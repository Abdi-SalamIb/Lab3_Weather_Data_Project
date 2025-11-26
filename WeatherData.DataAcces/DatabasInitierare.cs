using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WeatherData.DataAcces
{
    // Klass som ansvarar för att initiera databasen och importera data från CSV
    public class DatabasInitierare
    {
        private readonly VäderDbContext _kontext;

        // Konstruktor som tar emot databasens DbContext
        public DatabasInitierare(VäderDbContext kontext)
        {
            _kontext = kontext;
        }

        // Initierar databasen och importerar data från CSV
        public async Task InitieraAsync(string csvFilSökväg, bool tvingaOmimport = false)
        {
            Console.WriteLine("**************************");
            Console.WriteLine(" Initiering av databasen");
            Console.WriteLine("**************************");

            // Säkerställ att databasen är skapad
            Console.WriteLine(" Kontrollerar databasen...");
            await _kontext.Database.EnsureCreatedAsync();
            Console.WriteLine(" Databas klar.");
            Console.WriteLine();

            var befintligtAntal = await _kontext.VäderAvläsningar.CountAsync();

            // Om databasen redan innehåller data
            if (befintligtAntal > 0)
            {
                Console.WriteLine($"Databasen innehåller redan {befintligtAntal:N0} poster.");

                if (!tvingaOmimport)
                {
                    Console.WriteLine("Använder befintliga data.");
                    Console.WriteLine();
                    return;
                }

                // Ta bort befintliga data innan omimport
                Console.WriteLine("Tar bort befintliga data...");
                _kontext.VäderAvläsningar.RemoveRange(_kontext.VäderAvläsningar);
                await _kontext.SaveChangesAsync();
                Console.WriteLine("Data borttagna.");
                Console.WriteLine();
            }

            // Kontrollera att CSV-filen finns
            if (!File.Exists(csvFilSökväg))
            {
                Console.WriteLine($" FEL: Fil hittades inte: {csvFilSökväg}");
                Console.WriteLine("   Kontrollera att TempFuktData.csv finns i rätt mapp.");
                return;
            }

            Console.WriteLine($" Import från: {csvFilSökväg}");
            Console.WriteLine(" Läser och validerar filen...");
            Console.WriteLine();

            // Importera data från CSV
            var avläsningar = CsvImportTjänst.ImporteraFrånCsv(csvFilSökväg);

            if (avläsningar.Count == 0)
            {
                Console.WriteLine(" VARNING: Inga giltiga data hittades.");
                return;
            }

            const int batchStorlek = 1000; // Inför data i batcher för bättre prestanda
            int totaltInsatt = 0;

            Console.WriteLine($"Sätter in {avläsningar.Count:N0} poster...");
            Console.WriteLine("   (I partier om 1000 för prestanda)");
            Console.WriteLine();

            int progressSteg = avläsningar.Count / 10;

            // Sätt in poster i databasen i batchar
            for (int i = 0; i < avläsningar.Count; i += batchStorlek)
            {
                var batch = avläsningar.Skip(i).Take(batchStorlek).ToList();

                await _kontext.VäderAvläsningar.AddRangeAsync(batch);
                await _kontext.SaveChangesAsync();

                totaltInsatt += batch.Count;

                // Visa framsteg
                if (totaltInsatt % progressSteg < batchStorlek || totaltInsatt == avläsningar.Count)
                {
                    double procent = (totaltInsatt * 100.0) / avläsningar.Count;
                    Console.WriteLine($"   Framsteg: {totaltInsatt:N0}/{avläsningar.Count:N0} ({procent:F0}%)");
                }
            }

            Console.WriteLine("**************************");
            Console.WriteLine($" Import slutförd!");
            Console.WriteLine($"   Totalt: {totaltInsatt:N0} poster insatta");
            Console.WriteLine("**************************");
        }

        // Kollar om databasen innehåller några poster
        public async Task<bool> HarDataAsync()
        {
            return await _kontext.VäderAvläsningar.AnyAsync();
        }

        // Hämtar antal poster i databasen
        public async Task<int> HämtaAntalPosterAsync()
        {
            return await _kontext.VäderAvläsningar.CountAsync();
        }
    }
}
