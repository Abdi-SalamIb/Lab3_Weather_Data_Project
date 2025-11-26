using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherData.Core.Services
{
    // Klass för att beräkna mögelrisk baserat på temperatur och luftfuktighet
    public class MögelriskKalkylator
    {
        // Beräknar mögelrisk för en enskild temperatur och luftfuktighet
        public static double BeräknaMögelrisk(double temperatur, double luftfuktighet)
        {
            // Om luftfuktigheten är utanför giltigt intervall, returnera 0
            if (luftfuktighet < 0 || luftfuktighet > 100)
                return 0;

            // Normalisera luftfuktighet till faktor mellan 0 och 1
            double fuktighetsFaktor = luftfuktighet / 100.0;

            double tempFaktor;

            // Bestäm temperaturfaktor baserat på temperaturintervall
            if (temperatur < 0)
            {
                tempFaktor = 0.3; // Låg risk vid mycket låga temperaturer
            }
            else if (temperatur <= 20)
            {
                tempFaktor = 0.5 + (temperatur / 40.0); // Ökad risk med högre temperatur
            }
            else if (temperatur <= 30)
            {
                tempFaktor = 1.0; // Maximal risk vid måttlig temperatur
            }
            else
            {
                // Minskar risk vid hög temperatur
                tempFaktor = 1.0 - ((temperatur - 30) / 50.0);
                tempFaktor = Math.Max(0.2, tempFaktor); // Minsta temperaturfaktor = 0.2
            }

            // Grundläggande mögelrisk som procent
            double mögelrisk = fuktighetsFaktor * fuktighetsFaktor * tempFaktor * 100;

            // Öka risk vid hög luftfuktighet
            if (luftfuktighet > 70)
                mögelrisk *= 1.5;

            if (luftfuktighet > 80)
                mögelrisk *= 1.3;

            return Math.Round(mögelrisk, 2); // Returnera avrundad risk
        }

        // Beräknar medelvärde av mögelrisk för flera temperaturer och luftfuktigheter
        public static double BeräknaMedelMögelrisk(
            IEnumerable<double> temperaturer,
            IEnumerable<int> luftfuktigheter)
        {
            var tempLista = temperaturer.ToList();
            var fuktLista = luftfuktigheter.ToList();

            // Kontrollera att listorna har samma längd och inte är tomma
            if (tempLista.Count != fuktLista.Count || tempLista.Count == 0)
                return 0;

            // Beräkna risk för varje temperatur/fukt-par
            var risker = tempLista.Zip(fuktLista, (t, f) => BeräknaMögelrisk(t, f));

            // Returnera genomsnittlig risk avrundad till två decimaler
            return Math.Round(risker.Average(), 2);
        }
    }
}
