using FunBoardGames.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace FunBoardGames.Database
{
    public class FunBoardGamesDbContext : DbContext
    {
        public FunBoardGamesDbContext(DbContextOptions<FunBoardGamesDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserCredentials> UserCredentials => Set<UserCredentials>();
        public DbSet<BoardGameEntity> Games => Set<BoardGameEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserCredentials>(entity =>
            {
                entity.ToTable("UserCredentials");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.AuthTokenHash).IsRequired();
                entity.Property(e => e.DeviceId).IsRequired();
                entity.Property(e => e.JoinedAt).IsRequired();
            });

            modelBuilder.Entity<BoardGameEntity>(entity =>
            {
                entity.ToTable("Games");
                entity.UseTptMappingStrategy();
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GameType).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.PlayerCount).IsRequired();
            });

            modelBuilder.Entity<SETGameEntity>(entity =>
            {
                entity.ToTable("SETGames");
                entity.Property(e => e.GuessTime).IsRequired();
            });

            modelBuilder.Entity<CantStopGameEntity>(entity =>
            {
                entity.ToTable("CantStopGames");

                var boardData = entity.Property(e => e.BoardData)
                    .IsRequired()
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<Dictionary<int, int>>(v, (JsonSerializerOptions)null));

                boardData.Metadata.SetValueComparer(new ValueComparer<Dictionary<int, int>>(
                    (left, right) => (left ?? new()).SequenceEqual(right ?? new()),
                    dictionary => dictionary.Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.Key, pair.Value)),
                    dictionary => new Dictionary<int, int>(dictionary)));
            });
        }
    }
}
