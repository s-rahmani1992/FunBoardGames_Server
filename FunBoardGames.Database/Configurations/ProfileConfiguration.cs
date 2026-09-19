using FunBoardGames.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBoardGames.Database.Configurations
{
    internal class ProfileConfiguration : IEntityTypeConfiguration<Entities.Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> profileBuilder)
        {
            profileBuilder.ToTable("User-Profiles");
            profileBuilder.HasKey(p => p.Id);
            profileBuilder.HasOne<UserCredentials>()
                .WithOne()
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade).IsRequired(true);
            profileBuilder.Property(p => p.Id).HasColumnName("id");
            profileBuilder.Property(p => p.UserId).HasColumnName("user_id");
            profileBuilder.Property(p => p.PlayerName).HasColumnName("player_name").IsRequired(true);
            profileBuilder.Property(p => p.Avatar).HasColumnName("avatar").IsRequired(true).HasDefaultValue(0);
        }
    }
}
