using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TravelService.Impl.EF
{
    public class TravelServiceContext : DbContext
    {
        public TravelServiceContext(DbContextOptions<TravelServiceContext> options) : base(options)
        {

        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        public DbSet<PersistedLocation> PersistedLocations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersistedLocation>()
                .HasKey(v => v.Id);
            modelBuilder.Entity<PersistedLocation>()
                .Property(b => b.Attributes)
                .HasConversion(v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
        }

    }
}
