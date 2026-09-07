using FunBoardGames.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace FunBoardGames.Database
{
    public class FunBoardGamesDbContext : DbContext
    {
        public FunBoardGamesDbContext(DbContextOptions<FunBoardGamesDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserCredentials> UserCredentials => Set<UserCredentials>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserCredentials>(entity =>
            {
                entity.ToTable("UserCredentials");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.DeviceId).IsRequired();
                entity.Property(e => e.JoinedAt).IsRequired();
            });
        }
    }
}
