using Microsoft.EntityFrameworkCore;
using NBABets.Services;

namespace NBABets.API
{
    public class AppDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Bet> Bets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine($"Data Source={AppContext.BaseDirectory}", "NBABets.db");
            optionsBuilder.UseSqlite(dbPath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users table
            modelBuilder.Entity<User>().HasKey(u => u.ID);
            modelBuilder.Entity<User>()
                .Property(u => u.BetsPlaced)
                .IsRequired(false);
            // Games Tables
            modelBuilder.Entity<Game>().HasKey(g => g.ID);
            // Bets table
            modelBuilder.Entity<Bet>().HasKey(b => b.ID);
        }
    }
}
