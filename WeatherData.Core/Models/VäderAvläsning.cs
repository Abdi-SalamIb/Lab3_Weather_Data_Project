using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherData.Core.Models
{
    // Klass som representerar en väderavläsning
    public class VäderAvläsning
    {
        // Unikt ID för varje väderavläsning
        public int Id { get; set; }

        // Datum då avläsningen gjordes
        public DateTime Datum { get; set; }

        // Plats där avläsningen utfördes
        public string Plats { get; set; } = string.Empty;

        // Temperatur vid avläsningstillfället
        public double Temp { get; set; }

        // Luftfuktighet vid avläsningstillfället (i procent)
        public int Luftfuktighet { get; set; }
    }
}
