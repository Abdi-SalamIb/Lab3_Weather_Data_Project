using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherData.Core.Models;

namespace WeatherData.Core.Services
{
    // Klass som tillhandahåller olika metoder för analys av väderdata
    public class VäderAnalysTjänst
    {
        // Beräknar daglig statistik för en specifik plats
        public static List<DagligStatistik> BeräknaDagligStatistik(
            IEnumerable<VäderAvläsning> avläsningar,
            string plats)
        {
            // Filtrera avläsningar för vald plats, gruppera per dag och beräkna statistik
            var statistik = avläsningar
                .Where(a => a.Plats.Equals(plats, StringComparison.OrdinalIgnoreCase))
                .GroupBy(a => a.Datum.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DagligStatistik
                {
                    Datum = g.Key,
                    Plats = plats,
                    MedelTemp = Math.Round(g.Average(a => a.Temp), 2),
                    MedelLuftfuktighet = Math.Round(g.Average(a => a.Luftfuktighet), 2),
                    MinTemp = g.Min(a => a.Temp),
                    MaxTemp = g.Max(a => a.Temp),
                    Mögelrisk = MögelriskKalkylator.BeräknaMedelMögelrisk(
                        g.Select(a => a.Temp),
                        g.Select(a => a.Luftfuktighet))
                })
                .ToList();

            return statistik;
        }

        // Sorterar daglig statistik från varmast till kallast
        public static List<DagligStatistik> SorteraVarmasteTillKallaste(
            List<DagligStatistik> statistik)
        {
            return statistik.OrderByDescending(s => s.MedelTemp).ToList();
        }

        // Sorterar daglig statistik från torrast till fuktigast
        public static List<DagligStatistik> SorteraTorrasteTillFuktigaste(
            List<DagligStatistik> statistik)
        {
            return statistik.OrderBy(s => s.MedelLuftfuktighet).ToList();
        }

        // Sorterar daglig statistik baserat på mögelrisk (lägst till högst)
        public static List<DagligStatistik> SorteraMögelrisk(
            List<DagligStatistik> statistik)
        {
            return statistik.OrderBy(s => s.Mögelrisk).ToList();
        }

        // Hämtar medeltemperatur för en specifik dag och plats
        public static double? HämtaMedelTemperaturFörDatum(
            IEnumerable<VäderAvläsning> avläsningar,
            DateTime datum,
            string plats)
        {
            var dagensAvläsningar = avläsningar
                .Where(a => a.Datum.Date == datum.Date &&
                           a.Plats.Equals(plats, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!dagensAvläsningar.Any())
                return null;

            return Math.Round(dagensAvläsningar.Average(a => a.Temp), 2);
        }

        // Beräknar skillnader mellan inomhus- och utomhustemperatur per dag
        public static List<(DateTime Datum, double TempSkillnad)>
            BeräknaTemperaturSkillnader(IEnumerable<VäderAvläsning> avläsningar)
        {
            var skillnader = avläsningar
                .GroupBy(a => a.Datum.Date)
                .Select(g =>
                {
                    // Beräkna medeltemperatur inomhus
                    var inneMedel = g
                        .Where(a => a.Plats.Equals("Inne", StringComparison.OrdinalIgnoreCase))
                        .Average(a => a.Temp);

                    // Beräkna medeltemperatur utomhus
                    var uteMedel = g
                        .Where(a => a.Plats.Equals("Ute", StringComparison.OrdinalIgnoreCase))
                        .Average(a => a.Temp);

                    // Returnera datum och absolut temperaturskillnad
                    return (
                        Datum: g.Key,
                        TempSkillnad: Math.Round(Math.Abs(inneMedel - uteMedel), 2)
                    );
                })
                .OrderByDescending(s => s.TempSkillnad) // Sortera efter största skillnad
                .ToList();

            return skillnader;
        }
    }
}
