using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherData.Core.Models;

namespace WeatherData.Core.Services
{
    // Klass för att beräkna startdatum för olika meteorologiska säsonger
    public class MeteorologiskSäsongKalkylator
    {
        // Beräknar startdatum för höst baserat på medeltemperatur
        public static DateTime? BeräknaHöstStart(List<DagligStatistik> dagligStatistik)
        {
            const double höstGräns = 10.0; // Medeltemperaturgräns för höst
            const int konsekutivaDagar = 5; // Antal dagar som måste uppfylla kriteriet

            for (int i = 0; i <= dagligStatistik.Count - konsekutivaDagar; i++)
            {
                bool ärHöst = true;

                // Kontrollera om alla konsekutiva dagar ligger under höstgränsen
                for (int j = 0; j < konsekutivaDagar; j++)
                {
                    if (dagligStatistik[i + j].MedelTemp >= höstGräns)
                    {
                        ärHöst = false;
                        break;
                    }
                }

                if (ärHöst)
                    return dagligStatistik[i].Datum; // Returnera startdatum för höst
            }

            return null; // Ingen höststart hittades
        }

        // Beräknar startdatum för vinter baserat på medeltemperatur
        public static DateTime? BeräknaVinterStart(List<DagligStatistik> dagligStatistik)
        {
            const double vinterGräns = 0.0; // Medeltemperaturgräns för vinter
            const int konsekutivaDagar = 5;

            for (int i = 0; i <= dagligStatistik.Count - konsekutivaDagar; i++)
            {
                bool ärVinter = true;

                // Kontrollera om alla konsekutiva dagar ligger under vintergränsen
                for (int j = 0; j < konsekutivaDagar; j++)
                {
                    if (dagligStatistik[i + j].MedelTemp >= vinterGräns)
                    {
                        ärVinter = false;
                        break;
                    }
                }

                if (ärVinter)
                    return dagligStatistik[i].Datum; // Returnera startdatum för vinter
            }

            return null; // Ingen vinterstart hittades
        }

        // Beräknar startdatum för vår baserat på medeltemperatur
        public static DateTime? BeräknaVårStart(List<DagligStatistik> dagligStatistik)
        {
            const double vårGräns = 0.0; // Medeltemperaturgräns för vår
            const int konsekutivaDagar = 7;

            for (int i = 0; i <= dagligStatistik.Count - konsekutivaDagar; i++)
            {
                bool ärVår = true;

                // Kontrollera om alla konsekutiva dagar ligger över vårgränsen
                for (int j = 0; j < konsekutivaDagar; j++)
                {
                    if (dagligStatistik[i + j].MedelTemp <= vårGräns)
                    {
                        ärVår = false;
                        break;
                    }
                }

                if (ärVår)
                    return dagligStatistik[i].Datum; 
            }

            return null; 
        }

        // Beräknar startdatum för sommar baserat på medeltemperatur
        public static DateTime? BeräknaSommarStart(List<DagligStatistik> dagligStatistik)
        {
            const double sommarGräns = 10.0; // Medeltemperaturgräns för sommar
            const int konsekutivaDagar = 5;

            for (int i = 0; i <= dagligStatistik.Count - konsekutivaDagar; i++)
            {
                bool ärSommar = true;

                // Kontrollera om alla konsekutiva dagar ligger över sommargränsen
                for (int j = 0; j < konsekutivaDagar; j++)
                {
                    if (dagligStatistik[i + j].MedelTemp <= sommarGräns)
                    {
                        ärSommar = false;
                        break;
                    }
                }

                if (ärSommar)
                    return dagligStatistik[i].Datum; 
            }

            return null; 
        }
    }
}
