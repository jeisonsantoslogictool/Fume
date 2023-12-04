using fume.shared.Enttities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace fume.api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
                
        }

        public DbSet<Country> Countries { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().HasIndex(X => X.Name).IsUnique();
        }
    }
}
