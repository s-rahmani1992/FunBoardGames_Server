using FunBoardGames.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBoardGames.Database.Configurations
{
    public class SETGameConfiguration : IEntityTypeConfiguration<SETGameData>
    {
        public void Configure(EntityTypeBuilder<SETGameData> builder)
        {
            builder.ToTable("SET_Games");
            builder.Property(e => e.VisibleCardCount).HasColumnName("visible_card_count").IsRequired().HasDefaultValue(12);
        }
    }
}
