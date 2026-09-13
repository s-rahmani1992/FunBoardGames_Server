using FunBoardGames.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace FunBoardGames.Database.Configurations
{
    internal class CantStopGameConfiguration : IEntityTypeConfiguration<CantStopGameData>
    {
        public void Configure(EntityTypeBuilder<CantStopGameData> builder)
        {
            builder.ToTable("CantStop_Games");

            var boardDataProperty = builder.Property(e => e.BoardData)
                    .IsRequired()
                    .HasColumnType("jsonb").HasColumnName("board_data")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<Dictionary<int, int>>(v, (JsonSerializerOptions)null));

            boardDataProperty.Metadata.SetValueComparer(new ValueComparer<Dictionary<int, int>>(
                (left, right) => (left ?? new()).SequenceEqual(right ?? new()),
                dictionary => dictionary.Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.Key, pair.Value)),
                dictionary => new Dictionary<int, int>(dictionary)));
        }
    }
}
