using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherData.Core.Models
{
    // Klass som representerar dagliga väderstatistik
    public class DagligStatistik
    {
        // Datum då uppgifterna registrerades
        public DateTime Datum { get; set; }

        // Plats där uppgifterna samlades in
        public string Plats { get; set; } = string.Empty;

        // Dagens medeltemperatur
        public double MedelTemp { get; set; }

        // Medelrelativ luftfuktighet under dagen
        public double MedelLuftfuktighet { get; set; }

        // Dagens minsta temperatur
        public double MinTemp { get; set; }

        // Dagens högsta temperatur
        public double MaxTemp { get; set; }

        // Beräknad mögelrisk för dagen
        public double Mögelrisk { get; set; }
    }
}
