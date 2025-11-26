using WeatherData.Core.Models;

namespace WeatherData.Core.Services;

// Klass som detekterar öppningar av balkongdörr baserat på temperatur- och fuktighetsförändringar
public class BalkonDörrDetektor
{
    // Tröskelvärde för temperatursänkning inomhus
    private readonly double _temperaturTröskelvärde;

    // Tröskelvärde för ökning av luftfuktighet inomhus
    private readonly double _luftfuktighetsÖkning;

    // Tröskelvärde för temperaturökning utomhus
    private readonly double _uteTemperaturÖkning;

    // Konstruktor med standardvärden för trösklar
    public BalkonDörrDetektor(
        double temperaturTröskelvärde = 0.3,
        double luftfuktighetsÖkning = 1.5,
        double uteTemperaturÖkning = 0.1)
    {
        _temperaturTröskelvärde = temperaturTröskelvärde;
        _luftfuktighetsÖkning = luftfuktighetsÖkning;
        _uteTemperaturÖkning = uteTemperaturÖkning;
    }

    // Metod som detekterar dörröppningar och beräknar deras varaktighet
    public List<DörrÖppningMedVaraktighet> DetekteraÖppningarMedVaraktighet(
        List<VäderAvläsning> inneAvläsningar,
        List<VäderAvläsning> uteAvläsningar)
    {
        var resultat = new List<DörrÖppningMedVaraktighet>();

        // Sortera inomhusavläsningar på datum
        var sorteradeInne = inneAvläsningar
            .Where(a => a.Plats.ToLower() == "inne")
            .OrderBy(a => a.Datum)
            .ToList();

        // Sortera utomhusavläsningar på datum
        var sorteradeUte = uteAvläsningar
            .Where(a => a.Plats.ToLower() == "ute")
            .OrderBy(a => a.Datum)
            .ToList();

        // Skapa dictionary för snabba uppslag av utomhusavläsningar per minut
        var uteDictionary = new Dictionary<DateTime, VäderAvläsning>();
        foreach (var ute in sorteradeUte)
        {
            var roundedTime = new DateTime(ute.Datum.Year, ute.Datum.Month, ute.Datum.Day,
                                          ute.Datum.Hour, ute.Datum.Minute, 0);
            if (!uteDictionary.ContainsKey(roundedTime))
            {
                uteDictionary[roundedTime] = ute;
            }
        }

        // Loopa igenom inomhusavläsningar för att identifiera plötsliga förändringar
        for (int i = 1; i < sorteradeInne.Count; i++)
        {
            var inneFöregående = sorteradeInne[i - 1];
            var inneNuvarande = sorteradeInne[i];

            var tidsskillnad = (inneNuvarande.Datum - inneFöregående.Datum).TotalMinutes;

            // Endast jämför avläsningar som ligger inom 2 minuter
            if (tidsskillnad <= 2)
            {
                var inneTempFörändring = inneNuvarande.Temp - inneFöregående.Temp;
                var inneFuktFörändring = inneNuvarande.Luftfuktighet - inneFöregående.Luftfuktighet;

                // Runda av tider för att matcha med utomhusdata
                var roundedTimeFöregående = new DateTime(inneFöregående.Datum.Year, inneFöregående.Datum.Month,
                                                        inneFöregående.Datum.Day, inneFöregående.Datum.Hour,
                                                        inneFöregående.Datum.Minute, 0);
                var roundedTimeNuvarande = new DateTime(inneNuvarande.Datum.Year, inneNuvarande.Datum.Month,
                                                       inneNuvarande.Datum.Day, inneNuvarande.Datum.Hour,
                                                       inneNuvarande.Datum.Minute, 0);

                VäderAvläsning uteFöregående = null;
                VäderAvläsning uteNuvarande = null;

                // Hämta motsvarande utomhusavläsningar
                uteDictionary.TryGetValue(roundedTimeFöregående, out uteFöregående);
                uteDictionary.TryGetValue(roundedTimeNuvarande, out uteNuvarande);

                bool uteTemperaturÖkar = false;
                double uteTempFörändring = 0;

                if (uteFöregående != null && uteNuvarande != null)
                {
                    uteTempFörändring = uteNuvarande.Temp - uteFöregående.Temp;
                    uteTemperaturÖkar = uteTempFörändring > _uteTemperaturÖkning;
                }

                // Kontrollera om kriterier för dörröppning uppfylls
                if (inneTempFörändring < -_temperaturTröskelvärde &&
                    inneFuktFörändring > _luftfuktighetsÖkning &&
                    uteTemperaturÖkar)
                {
                    var varaktighet = BeräknaVaraktighet(sorteradeInne, i);

                    // Lägg till en detekterad dörröppning med varaktighet
                    resultat.Add(new DörrÖppningMedVaraktighet
                    {
                        ÖppningsDatum = inneNuvarande.Datum,
                        StängningsDatum = inneNuvarande.Datum.AddMinutes(varaktighet),
                        VaraktighetMinuter = varaktighet,
                        TemperaturFallInne = Math.Abs(inneTempFörändring),
                        LuftfuktighetsÖkning = inneFuktFörändring,
                        TemperaturÖkningUte = uteTempFörändring,
                        InneTempFörut = inneFöregående.Temp,
                        InneTempEfter = inneNuvarande.Temp,
                        UteTempFörut = uteFöregående?.Temp ?? 0,
                        UteTempEfter = uteNuvarande?.Temp ?? 0
                    });
                }
            }
        }

        return resultat;
    }

    // Beräknar varaktigheten för en dörröppning baserat på temperaturåterställning
    private int BeräknaVaraktighet(List<VäderAvläsning> sorteradeAvläsningar, int öppningsIndex)
    {
        var öppningsTemp = sorteradeAvläsningar[öppningsIndex].Temp;
        var normalTemp = sorteradeAvläsningar[öppningsIndex - 1].Temp;
        var temperaturSkillnad = normalTemp - öppningsTemp;

        if (temperaturSkillnad == 0) return 1;

        int varaktighet = 1;

        // Detekterar stängning när temperaturen återgår till 70% av normal
        for (int j = öppningsIndex + 1; j < sorteradeAvläsningar.Count && j < öppningsIndex + 30; j++)
        {
            var aktuellTemp = sorteradeAvläsningar[j].Temp;
            var återställdProcent = Math.Abs((normalTemp - aktuellTemp) / temperaturSkillnad);

            if (återställdProcent < 0.3)
            {
                break;
            }

            varaktighet++;
        }

        return varaktighet;
    }

    // Grupperar dörröppningar per dag och beräknar statistik
    public Dictionary<DateTime, DagligDörrStatistik> GrupperaÖppningarPerDag(List<DörrÖppningMedVaraktighet> öppningar)
    {
        return öppningar
            .GroupBy(ö => ö.ÖppningsDatum.Date)
            .ToDictionary(
                g => g.Key,
                g => new DagligDörrStatistik
                {
                    Datum = g.Key,
                    AntalÖppningar = g.Count(),
                    TotalVaraktighetMinuter = g.Sum(ö => ö.VaraktighetMinuter),
                    GenomsnittligVaraktighet = g.Average(ö => ö.VaraktighetMinuter),
                    LängstaÖppning = g.Max(ö => ö.VaraktighetMinuter)
                });
    }
}

// Klass för dörröppning utan varaktighet
public class DörrÖppning
{
    public DateTime ÖppningsDatum { get; set; }
    public double TemperaturFall { get; set; }
    public double LuftfuktighetsÖkning { get; set; }
    public double TempFörut { get; set; }
    public double TempEfter { get; set; }
    public double FuktFörut { get; set; }
    public double FuktEfter { get; set; }
}

// Klass för dörröppning med beräknad varaktighet
public class DörrÖppningMedVaraktighet
{
    public DateTime ÖppningsDatum { get; set; }
    public DateTime StängningsDatum { get; set; }
    public int VaraktighetMinuter { get; set; }
    public double TemperaturFallInne { get; set; }
    public double LuftfuktighetsÖkning { get; set; }
    public double TemperaturÖkningUte { get; set; }
    public double InneTempFörut { get; set; }
    public double InneTempEfter { get; set; }
    public double UteTempFörut { get; set; }
    public double UteTempEfter { get; set; }
}

// Klass för sammanfattad statistik per dag
public class DagligDörrStatistik
{
    public DateTime Datum { get; set; }
    public int AntalÖppningar { get; set; }
    public int TotalVaraktighetMinuter { get; set; }
    public double GenomsnittligVaraktighet { get; set; }
    public int LängstaÖppning { get; set; }
}
