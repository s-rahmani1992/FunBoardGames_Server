using FunBoardGames.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBoardGames.Database.Configurations
{
    public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredentials>
    {
        public void Configure(EntityTypeBuilder<UserCredentials> userCredentialBuilder)
        {
            userCredentialBuilder.ToTable("User_Credentials");
            userCredentialBuilder.HasKey(credential => credential.Id);
            userCredentialBuilder.Property(credential => credential.Id).HasColumnName("id");
            userCredentialBuilder.Property(credential => credential.Name).HasColumnName("name").IsRequired();
            userCredentialBuilder.Property(credential => credential.AuthTokenHash).HasColumnName("auth_token_hash").IsRequired();
            userCredentialBuilder.Property(credential => credential.DeviceId).HasColumnName("device_id").IsRequired();
            userCredentialBuilder.Property(credential => credential.JoinedAt).HasColumnName("joined_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            userCredentialBuilder.Property(credential => credential.LastLoginAt).HasColumnName("last_login_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            userCredentialBuilder.Property(credential => credential.DeletedAt).HasColumnName("deleted_at").IsRequired(false);
        }
    }
}
