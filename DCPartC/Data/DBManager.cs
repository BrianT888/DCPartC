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
            base.OnModelCreating(modelBuilder);
        }
    }
}
