using Microsoft.EntityFrameworkCore;
using DCPartC.Models;

namespace DCPartC.Data
{
    public class DBManager : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source = Database.db;");
        }
        public DbSet<Client> Client { get; set; }
        public DbSet<Job> Job { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Job>()
                .HasOne<Client>()
                .WithMany()
                .HasForeignKey(j => j.clientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
