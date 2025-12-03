using CrmUtility.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmUtility.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<OAuthConnection> OAuthConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<OAuthConnection>()
                .Property(x => x.Crm)
                .HasConversion<int>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
