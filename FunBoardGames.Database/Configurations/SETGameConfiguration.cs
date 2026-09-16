using FunBoardGames.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBoardGames.Database.Configurations
{
    public class SETGameConfiguration : IEntityTypeConfiguration<SETGameData>
    {
        public void Configure(EntityTypeBuilder<SETGameData> builder)
        {
            // A null attribute varies across the deck; otherwise it is constant with a value from 0 to 2.
            builder.ToTable("SET_Games", table =>
            {
                table.HasCheckConstraint("CK_SET_Games_color_attribute", "color_attribute BETWEEN 0 AND 2");
                table.HasCheckConstraint("CK_SET_Games_shape_attribute", "shape_attribute BETWEEN 0 AND 2");
                table.HasCheckConstraint("CK_SET_Games_count_attribute", "count_attribute BETWEEN 0 AND 2");
                table.HasCheckConstraint("CK_SET_Games_shading_attribute", "shading_attribute BETWEEN 0 AND 2");
                table.HasCheckConstraint("CK_SET_Games_round_time", "round_time > 0");
                table.HasCheckConstraint("CK_SET_Games_wrong_limit", "wrong_limit > 0");
            });
            builder.Property(e => e.VisibleCardCount).HasColumnName("visible_card_count").IsRequired().HasDefaultValue(12);
            builder.Property(e => e.RoundTime).HasColumnName("round_time").IsRequired(true).HasDefaultValue(60);
            builder.Property(e => e.WrongLimit).HasColumnName("wrong_limit").IsRequired(true).HasDefaultValue(3);
            builder.Property(e => e.ColorAttribute).HasColumnName("color_attribute").IsRequired(false).HasDefaultValue(null);
            builder.Property(e => e.ShapeAttribute).HasColumnName("shape_attribute").IsRequired(false).HasDefaultValue(null);
            builder.Property(e => e.CountAttribute).HasColumnName("count_attribute").IsRequired(false).HasDefaultValue(null);
            builder.Property(e => e.ShadingAttribute).HasColumnName("shading_attribute").IsRequired(false).HasDefaultValue(null);
        }
    }
}
