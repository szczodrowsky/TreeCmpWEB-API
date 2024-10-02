using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using TreeCmpWebAPI.Models.Domain;

namespace TreeCmpWebAPI.Data
{
    public class NewickDbContext : DbContext
    {
        public NewickDbContext(DbContextOptions<NewickDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<Newick> Newicks { get; set; }

        public DbSet<NewickResponseFile> ResponseFiles { get; set; } 



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var stringArrayConverter = new ValueConverter<string[], string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null));

            modelBuilder.Entity<Newick>()
                .Property(e => e.rootedMetrics)
                .HasConversion(stringArrayConverter);

            modelBuilder.Entity<Newick>()
                .Property(e => e.unrootedMetrics)
                .HasConversion(stringArrayConverter);
            modelBuilder.Entity<NewickResponseFile>().ToTable("ResponseFiles");

        }
    }
}
