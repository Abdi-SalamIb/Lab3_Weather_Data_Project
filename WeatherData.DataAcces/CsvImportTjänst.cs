using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using WeatherData.Core.Models;

namespace WeatherData.DataAcces
{
    // Klass som hanterar import av väderavläsningar från CSV-filer
    public class CsvImportTjänst
    {
        // Intern klass som representerar en rad i CSV-filen
        private class CsvVäderAvläsning
        {
            public string Datum { get; set; } = string.Empty;
            public string Plats { get; set; } = string.Empty;
            public string Temp { get; set; } = string.Empty;
            public string Luftfuktighet { get; set; } = string.Empty;
        }

        // Mappning mellan CSV-kolumner och CsvVäderAvläsning-egenskaper
        private class CsvVäderAvläsningMap : ClassMap<CsvVäderAvläsning>
        {
            public CsvVäderAvläsningMap()
            {
                Map(m => m.Datum).Name("Datum");
                Map(m => m.Plats).Name("Plats");
                Map(m => m.Temp).Name("Temp");
                Map(m => m.Luftfuktighet).Name("Luftfuktighet");
            }
        }

        // Importerar väderavläsningar från en CSV-fil
        public static List<VäderAvläsning> ImporteraFrånCsv(string filSökväg)
        {
            var avläsningar = new List<VäderAvläsning>();
            var kultur = CultureInfo.InvariantCulture;

            using var läsare = new StreamReader(filSökväg);

            // Konfiguration för CsvHelper
            var config = new CsvConfiguration(kultur)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var csv = new CsvReader(läsare, config);
            csv.Context.RegisterClassMap<CsvVäderAvläsningMap>();

            var poster = csv.GetRecords<CsvVäderAvläsning>();

            int radNummer = 1;

            // Loopa igenom varje rad i CSV-filen
            foreach (var post in poster)
            {
                radNummer++;

                try
                {
                    var väderAvläsning = TolkaOchValideraPost(post, radNummer);

                    if (väderAvläsning != null)
                        avläsningar.Add(väderAvläsning);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Fel rad {radNummer}: {ex.Message}");
                }
            }

            Console.WriteLine($" Importerade {avläsningar.Count} giltiga avläsningar");
            return avläsningar;
        }

        // Tolkar och validerar en rad från CSV-filen
        private static VäderAvläsning? TolkaOchValideraPost(
            CsvVäderAvläsning post,
            int radNummer)
        {
            // Kontrollera datum
            if (!DateTime.TryParse(post.Datum, CultureInfo.InvariantCulture,
                out DateTime datum))
            {
                Console.WriteLine($" Rad {radNummer}: Ogiltigt datum '{post.Datum}'");
                return null;
            }

            string plats = post.Plats.Trim();

            // Kontrollera att plats är "Ute" eller "Inne"
            if (plats.Equals("Ute", StringComparison.OrdinalIgnoreCase))
            {
                plats = "Ute";
            }
            else if (plats.Equals("Inne", StringComparison.OrdinalIgnoreCase))
            {
                plats = "Inne";
            }
            else
            {
                Console.WriteLine($"Rad {radNummer}: Ogiltig plats '{plats}'");
                return null;
            }

            // Konvertera temperatur till double
            string tempStr = post.Temp.Replace(',', '.');

            if (!double.TryParse(tempStr, NumberStyles.Any,
                CultureInfo.InvariantCulture, out double temp))
            {
                Console.WriteLine($"Rad {radNummer}: Ogiltig temperatur '{post.Temp}'");
                return null;
            }

            // Kontrollera temperaturgränser
            if (temp < -50 || temp > 60)
            {
                Console.WriteLine($"Rad {radNummer}: Temperatur utanför gränser ({temp}°C)");
                return null;
            }

            // Konvertera luftfuktighet till int
            if (!int.TryParse(post.Luftfuktighet, out int luftfuktighet))
            {
                Console.WriteLine($"Rad {radNummer}: Ogiltig luftfuktighet '{post.Luftfuktighet}'");
                return null;
            }

            // Kontrollera luftfuktighetsgränser
            if (luftfuktighet < 0 || luftfuktighet > 100)
            {
                Console.WriteLine($"Rad {radNummer}: Luftfuktighet utanför gränser ({luftfuktighet}%)");
                return null;
            }

            // Skapa och returnera en VäderAvläsning
            return new VäderAvläsning
            {
                Datum = datum,
                Plats = plats,
                Temp = temp,
                Luftfuktighet = luftfuktighet
            };
        }
    }
}
