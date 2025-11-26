using Microsoft.EntityFrameworkCore;
using System.Text;
using WeatherData.Core.Models;
using WeatherData.Core.Services;
using WeatherData.DataAcces;

namespace WeatherData.UI;

class Program
{
    private static List<VäderAvläsning> allaAvläsningar;
    private static List<VäderAvläsning> uteAvläsningar;
    private static List<VäderAvläsning> inneAvläsningar;
    private static List<DagligStatistik> uteStatistik;
    private static List<DagligStatistik> inneStatistik;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        SkrivHuvud();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Initierar...\n");
        Console.ResetColor();

        using var context = new VäderDbContext();
        var initierare = new DatabasInitierare(context);

        await initierare.InitieraAsync("TempFuktData.csv", tvingaOmimport: false);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n Analyserar data...");
        Console.ResetColor();

        allaAvläsningar = await context.VäderAvläsningar.ToListAsync();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  {allaAvläsningar.Count:N0} avläsningar laddade");
        Console.ResetColor();

        uteAvläsningar = allaAvläsningar.Where(a => a.Plats.ToLower() == "ute").ToList();
        inneAvläsningar = allaAvläsningar.Where(a => a.Plats.ToLower() == "inne").ToList();

        uteStatistik = VäderAnalysTjänst.BeräknaDagligStatistik(uteAvläsningar, "Ute");
        inneStatistik = VäderAnalysTjänst.BeräknaDagligStatistik(inneAvläsningar, "Inne");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"    {uteStatistik.Count} dagar analyserade (utomhus)");
        Console.WriteLine($"    {inneStatistik.Count} dagar analyserade (inomhus)");
        Console.ResetColor();

        ShowMenu();
    }

    static void ShowMenu()
    {
        bool körProgram = true;

        while (körProgram)
        {
            Console.Clear();
            SkrivHuvud();

            Console.WriteLine("\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-------- HUVUDMENY ----------");
            Console.WriteLine("\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  1.  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Visa Utomhusanalyser (UTE)");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  2.  ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Visa Inomhusanalyser (INNE)");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  3.  ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Visa Jämförande Analyser");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  4.  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Visa Balkondörr Öppningar ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  5.  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Sök efter specifik datum (UTE)");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  6.  ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Sök efter specifik datum (INNE)");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  7.  ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Visa ALLA Analyser");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  8.  ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Avsluta\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   Välj ett alternativ (1-8): ");
            Console.ResetColor();
            var val = Console.ReadLine();

            switch (val)
            {
                case "1":
                    Console.Clear();
                    VisaUtomhusAnalyser(uteStatistik);
                    VäntaPåAnvändare();
                    break;

                case "2":
                    Console.Clear();
                    VisaInomhusAnalyser(inneStatistik);
                    VäntaPåAnvändare();
                    break;

                case "3":
                    Console.Clear();
                    VisaJämförandeAnalyser(allaAvläsningar);
                    VäntaPåAnvändare();
                    break;

                case "4":
                    Console.Clear();
                    VisaBalkonDörrAnalyser(inneAvläsningar, uteAvläsningar);
                    VäntaPåAnvändare();
                    break;

                case "5":
                    SökEfterDatum(uteStatistik, "Ute");
                    VäntaPåAnvändare();
                    break;

                case "6":
                    SökEfterDatum(inneStatistik, "Inne");
                    VäntaPåAnvändare();
                    break;

                case "7":
                    Console.Clear();
                    VisaUtomhusAnalyser(uteStatistik);
                    VisaInomhusAnalyser(inneStatistik);
                    VisaJämförandeAnalyser(allaAvläsningar);
                    VisaBalkonDörrAnalyser(inneAvläsningar, uteAvläsningar);
                    SkrivFot();
                    VäntaPåAnvändare();
                    break;

                case "8":
                    körProgram = false;
                    Console.Clear();
                    SkrivHuvud();
                    Console.WriteLine("\n");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  Tack och hej då!  ");
                    Console.ResetColor();
                    Console.WriteLine("\n");
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n Ogiltigt val. Försök igen.");
                    Console.ResetColor();
                    Thread.Sleep(1500);
                    break;
            }
        }
    }

    static void VäntaPåAnvändare()
    {
        Console.WriteLine("\n");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("------------- Tryck på valfri tangent för meny...---------");
        Console.ResetColor();
        Console.ReadKey();
    }

    static void SkrivHuvud()
    {
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("LAB 3: PROJEKT VÄDERDATA");
        Console.ResetColor();
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Förberett av eleven Abdi-Salam Ibrahim");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(" DevSecOps 2025, Khy ");
        Console.ResetColor();
        Console.WriteLine("");


        Console.WriteLine();
    }

    static void SkrivFot()
    {
        Console.WriteLine("\n");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(" ------------------- ANALYS SLUTFÖRD FRAMGÅNGSRIKT -------------------- ");
        Console.ResetColor();
        Console.WriteLine("");
    }

    static void VisaUtomhusAnalyser(List<DagligStatistik> uteStatistik)
    {
        Console.WriteLine("\n");
        Console.WriteLine("\n");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" ------------------- UTOMHUSANALYSER (UTE) -------------------- ");
        Console.ResetColor();
        Console.WriteLine("");



        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  TOPP 10 - Varmaste dagarna (utomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var varmaste = VäderAnalysTjänst.SorteraVarmasteTillKallaste(uteStatistik).Take(10).ToList();
        for (int i = 0; i < varmaste.Count; i++)
        {
            var dag = varmaste[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{dag.MedelTemp,6:F2}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Min: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{dag.MinTemp,6:F1}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Max: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{dag.MaxTemp,6:F1}°C");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("\n  TOPP 10 - Kallaste dagarna (utomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var kallaste = VäderAnalysTjänst.SorteraVarmasteTillKallaste(uteStatistik).TakeLast(10).Reverse().ToList();
        for (int i = 0; i < kallaste.Count; i++)
        {
            var dag = kallaste[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"{dag.MedelTemp,6:F2}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Min: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.MinTemp,6:F1}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Max: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{dag.MaxTemp,6:F1}°C");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n  TOPP 10 - Torraste dagarna (utomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var torraste = VäderAnalysTjänst.SorteraTorrasteTillFuktigaste(uteStatistik).Take(10).ToList();
        for (int i = 0; i < torraste.Count; i++)
        {
            var dag = torraste[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Luftfuktighet: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{dag.MedelLuftfuktighet,5:F1}%");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n TOPP 10 - Fuktigaste dagarna (utomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var fuktigaste = VäderAnalysTjänst.SorteraTorrasteTillFuktigaste(uteStatistik).TakeLast(10).Reverse().ToList();
        for (int i = 0; i < fuktigaste.Count; i++)
        {
            var dag = fuktigaste[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Luftfuktighet: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{dag.MedelLuftfuktighet,5:F1}%");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n TOPP 10 - Största mögelrisk (utomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var mögelrisk = VäderAnalysTjänst.SorteraMögelrisk(uteStatistik).TakeLast(10).Reverse().ToList();
        for (int i = 0; i < mögelrisk.Count; i++)
        {
            var dag = mögelrisk[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Risk: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"{dag.Mögelrisk,6:F2}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Temp: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{dag.MedelTemp,6:F1}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Fukt: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{dag.MedelLuftfuktighet,5:F1}%");
            Console.ResetColor();
        }

        var höstStart = MeteorologiskSäsongKalkylator.BeräknaHöstStart(uteStatistik);
        var vinterStart = MeteorologiskSäsongKalkylator.BeräknaVinterStart(uteStatistik);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n METEOROLOGISKA SÄSONGER:");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();

        if (höstStart.HasValue)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("    Höst: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Börjar den ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{höstStart.Value:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("      (Temperatur < 10°C i 5 dagar i rad)");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("    Höst: Ej hittad");
            Console.ResetColor();
        }

        Console.WriteLine();
        if (vinterStart.HasValue)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("     Vinter: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Börjar den ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{vinterStart.Value:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("      (Temperatur < 0°C i 5 dagar i rad)");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("     Vinter: Ej hittad (vintern 2016 var mild)");
            Console.WriteLine("      (Ingen period med 5 dagar < 0°C hittades)");
            Console.ResetColor();
        }
    }

    static void VisaInomhusAnalyser(List<DagligStatistik> inneStatistik)
    {
        Console.WriteLine("\n\n");

        Console.WriteLine("\n");
        Console.WriteLine("\n");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(" ------------------- INOMHUSANALYSER (INNE) -------------------- ");
        Console.ResetColor();
        Console.WriteLine("");




        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  TOPP 10 - Varmaste dagarna (inomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var varmaste = VäderAnalysTjänst.SorteraVarmasteTillKallaste(inneStatistik).Take(10).ToList();
        for (int i = 0; i < varmaste.Count; i++)
        {
            var dag = varmaste[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{dag.MedelTemp,6:F2}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Min: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{dag.MinTemp,6:F1}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Max: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{dag.MaxTemp,6:F1}°C");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("\n  TOPP 10 - Kallaste dagarna (inomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var kallaste = VäderAnalysTjänst.SorteraVarmasteTillKallaste(inneStatistik).TakeLast(10).Reverse().ToList();
        for (int i = 0; i < kallaste.Count; i++)
        {
            var dag = kallaste[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"{dag.MedelTemp,6:F2}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Min: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.MinTemp,6:F1}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Max: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{dag.MaxTemp,6:F1}°C");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n  TOPP 10 - Torraste dagarna (inomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var torraste = VäderAnalysTjänst.SorteraTorrasteTillFuktigaste(inneStatistik).Take(10).ToList();
        for (int i = 0; i < torraste.Count; i++)
        {
            var dag = torraste[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Luftfuktighet: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{dag.MedelLuftfuktighet,5:F1}%");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n TOPP 10 - Fuktigaste dagarna (inomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var fuktigaste = VäderAnalysTjänst.SorteraTorrasteTillFuktigaste(inneStatistik).TakeLast(10).Reverse().ToList();
        for (int i = 0; i < fuktigaste.Count; i++)
        {
            var dag = fuktigaste[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Luftfuktighet: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{dag.MedelLuftfuktighet,5:F1}%");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n TOPP 10 - Största mögelrisk (inomhus):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var mögelrisk = VäderAnalysTjänst.SorteraMögelrisk(inneStatistik).TakeLast(10).Reverse().ToList();
        for (int i = 0; i < mögelrisk.Count; i++)
        {
            var dag = mögelrisk[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dag.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Risk: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"{dag.Mögelrisk,6:F2}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Temp: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{dag.MedelTemp,6:F1}°C");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Fukt: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{dag.MedelLuftfuktighet,5:F1}%");
            Console.ResetColor();
        }
    }

    static void VisaJämförandeAnalyser(List<VäderAvläsning> allaAvläsningar)
    {
        Console.WriteLine("\n\n");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(" ------------------- JÄMFÖRANDE ANALYSER-------------------- ");
        Console.ResetColor();
        Console.WriteLine("");


        var skillnader = VäderAnalysTjänst.BeräknaTemperaturSkillnader(allaAvläsningar);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  TOPP 10 - Största temperaturskillnader:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("      (Inomhus vs Utomhus)");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var störstaSkillnader = skillnader.OrderByDescending(s => s.TempSkillnad).Take(10).ToList();
        for (int i = 0; i < störstaSkillnader.Count; i++)
        {
            var s = störstaSkillnader[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{s.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Skillnad: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{s.TempSkillnad:F2}°C");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n  TOPP 10 - Minsta temperaturskillnader:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("      (Inomhus vs Utomhus)");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("───────────────────────────────────────────────────────");
        Console.ResetColor();
        var minstaSkillnader = skillnader.OrderBy(s => s.TempSkillnad).Take(10).ToList();
        for (int i = 0; i < minstaSkillnader.Count; i++)
        {
            var s = minstaSkillnader[i];
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   {i + 1,2}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{s.Datum:yyyy-MM-dd}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" │ Skillnad: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{s.TempSkillnad:F2}°C");
            Console.ResetColor();
        }
    }

    static void VisaBalkonDörrAnalyser(List<VäderAvläsning> inneAvläsningar, List<VäderAvläsning> uteAvläsningar)
    {
        double tempSeuil = 0.3;
        double fuktSeuil = 1.5;
        double uteTempSeuil = 0.1;

        var dörrDetektor = new BalkonDörrDetektor(
            temperaturTröskelvärde: tempSeuil,
            luftfuktighetsÖkning: fuktSeuil,
            uteTemperaturÖkning: uteTempSeuil);

        Console.WriteLine("\n\n");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(" ------------------- BALKONDÖRR ÖPPNINGAR -------------------- ");
        Console.ResetColor();
        Console.WriteLine("");


        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n Detekterar balkondörröppningar med varaktighet...");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("    Kriterier:");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"    • Temperaturfall inne > {tempSeuil}°C");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"    • Luftfuktighetsökning inne > {fuktSeuil}%");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"    • Temperaturökning ute > {uteTempSeuil}°C");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("    • Alla tre samtidigt mellan minuter");
        Console.ResetColor();

        var öppningar = dörrDetektor.DetekteraÖppningarMedVaraktighet(inneAvläsningar, uteAvläsningar);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n Totalt detekterade öppningar: {öppningar.Count}");
        Console.ResetColor();

        if (öppningar.Count > 0)
        {
            var dagStatistik = dörrDetektor.GrupperaÖppningarPerDag(öppningar);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n TOPP 10 - Dagar med längst total öppningstid:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("───────────────────────────────────────────────────────");
            Console.ResetColor();
            var top10Varaktighet = dagStatistik
                .OrderByDescending(kvp => kvp.Value.TotalVaraktighetMinuter)
                .Take(10)
                .ToList();

            for (int i = 0; i < top10Varaktighet.Count; i++)
            {
                var dag = top10Varaktighet[i];
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"   {i + 1,2}. ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{dag.Key:yyyy-MM-dd}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" │ ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{dag.Value.TotalVaraktighetMinuter,3} min");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" │ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{dag.Value.AntalÖppningar} öppningar");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n TOPP 10 - Dagar med flest öppningar:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("───────────────────────────────────────────────────────");
            Console.ResetColor();
            var top10Antal = dagStatistik
                .OrderByDescending(kvp => kvp.Value.AntalÖppningar)
                .Take(10)
                .ToList();

            for (int i = 0; i < top10Antal.Count; i++)
            {
                var dag = top10Antal[i];
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"   {i + 1,2}. ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{dag.Key:yyyy-MM-dd}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" │ ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($"{dag.Value.AntalÖppningar,3} öppningar");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" │ ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{dag.Value.TotalVaraktighetMinuter} min totalt");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n TOPP 10 - Längsta enskilda öppningar:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("───────────────────────────────────────────────────────");
            Console.ResetColor();
            var top10Längsta = öppningar
                .OrderByDescending(ö => ö.VaraktighetMinuter)
                .Take(10)
                .ToList();

            for (int i = 0; i < top10Längsta.Count; i++)
            {
                var ö = top10Längsta[i];
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"   {i + 1,2}. ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{ö.ÖppningsDatum:yyyy-MM-dd HH:mm}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" → ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{ö.StängningsDatum:HH:mm}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"       • Varaktighet: {ö.VaraktighetMinuter} minuter");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"       • Temp inne: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{ö.InneTempFörut:F1}°C → {ö.InneTempEfter:F1}°C ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"(↓{ö.TemperaturFallInne:F1}°C)");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"       • Temp ute: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{ö.UteTempFörut:F1}°C → {ö.UteTempEfter:F1}°C ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"(↑{ö.TemperaturÖkningUte:F1}°C)");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"       • Fukt inne: ↑{ö.LuftfuktighetsÖkning:F1}%");
                Console.ResetColor();
            }

            var totalMinuter = öppningar.Sum(ö => ö.VaraktighetMinuter);
            var genomsnittVaraktighet = öppningar.Average(ö => ö.VaraktighetMinuter);
            var längstaÖppning = öppningar.Max(ö => ö.VaraktighetMinuter);
            var kortasteÖppning = öppningar.Min(ö => ö.VaraktighetMinuter);
            var genomsnittTempFall = öppningar.Average(ö => ö.TemperaturFallInne);
            var genomsnittTempÖkningUte = öppningar.Average(ö => ö.TemperaturÖkningUte);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n STATISTIK:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("───────────────────────────────────────────────────────");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   • Totalt antal öppningar: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{öppningar.Count}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   • Total öppningstid: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{totalMinuter} minuter ({totalMinuter / 60.0:F1} timmar)");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   • Genomsnittlig varaktighet: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{genomsnittVaraktighet:F1} minuter");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   • Längsta öppning: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{längstaÖppning} minuter");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   • Kortaste öppning: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{kortasteÖppning} minuter");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   • Antal dagar med öppningar: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{dagStatistik.Count}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   • Genomsnittlig temperaturfall inne: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{genomsnittTempFall:F2}°C");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   • Genomsnittlig temperaturökning ute: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{genomsnittTempÖkningUte:F2}°C");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  Inga balkondörröppningar detekterade med nuvarande kriterier.");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"    Tröskelvärden: Temp inne > {tempSeuil}°C, Fukt > {fuktSeuil}%, Temp ute > {uteTempSeuil}°C");
            Console.ResetColor();
        }
    }
    static void SökEfterDatum(List<DagligStatistik> statistik, string plats)
    {
        Console.Clear();
        Console.WriteLine("\n\n");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("---------------SÖK EFTER DATUM --------------- ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"({plats.ToUpper()}) ");
        Console.ResetColor();
        Console.WriteLine(" \n");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("  Ange datum (ÅÅÅÅ-MM-DD): ");
        Console.ResetColor();
        var input = Console.ReadLine();

        if (DateTime.TryParse(input, out DateTime söktDatum))
        {
            var resultat = statistik.FirstOrDefault(s => s.Datum.Date == söktDatum.Date);

            if (resultat != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n RESULTAT HITTADES:");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("───────────────────────────────────────────────────────");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("   Datum: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{resultat.Datum:yyyy-MM-dd}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("   Medeltemperatur: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{resultat.MedelTemp:F2}°C");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("    Min temperatur: ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{resultat.MinTemp:F1}°C");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("   Max temperatur: ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{resultat.MaxTemp:F1}°C");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("   Medel luftfuktighet: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{resultat.MedelLuftfuktighet:F1}%");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("   Mögelrisk: ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{resultat.Mögelrisk:F2}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("───────────────────────────────────────────────────────");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n Inget data hittades för datum {söktDatum:yyyy-MM-dd}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("   Kontrollera att datumet är inom perioden 2016-10-01 till 2016-11-30.");
                Console.ResetColor();
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n Ogiltigt datumformat. Använd formatet ÅÅÅÅ-MM-DD (t.ex. 2016-10-15)");
            Console.ResetColor();
        }
    }
}