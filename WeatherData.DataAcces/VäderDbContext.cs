using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherData.Core.Models;

namespace WeatherData.DataAcces
{
    // DbContext-klass för att hantera väderavläsningar i databasen
    public class VäderDbContext : DbContext
    {
        // DbSet som representerar tabellen för väderavläsningar
        public DbSet<VäderAvläsning> VäderAvläsningar { get; set; }

        // Standardkonstruktor
        public VäderDbContext() { }

        // Konstruktor med DbContextOptions (t.ex. för dependency injection)
        public VäderDbContext(DbContextOptions<VäderDbContext> options)
            : base(options) { }

        // Konfigurera databaskoppling om den inte redan är konfigurerad
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Använder SQLite som databas
                optionsBuilder.UseSqlite("Data Source=VäderData.db");
            }
        }

        // Konfigurera databasens modell och index
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VäderAvläsning>(entity =>
            {
                // Primärnyckel
                entity.HasKey(e => e.Id);

                // Index för datum för snabbare sökningar
                entity.HasIndex(e => e.Datum);

                // Index för plats
                entity.HasIndex(e => e.Plats);

                // Kombinerat index för datum och plats
                entity.HasIndex(e => new { e.Datum, e.Plats });
            });
        }
    }
}
