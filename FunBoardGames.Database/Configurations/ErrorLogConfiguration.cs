
using FunBoardGames.Domain.ErrorHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBoardGames.Database.Configurations
{
    public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
    {
        public void Configure(EntityTypeBuilder<ErrorLog> errorLogBuilder)
        {
            errorLogBuilder.ToTable("Error_Logs");
            errorLogBuilder.HasKey(log => log.Id);
            errorLogBuilder.Property(log => log.Id).HasColumnName("id");
            errorLogBuilder.Property(log => log.Type).HasColumnName("type").IsRequired();
            errorLogBuilder.Property(log => log.UserId).HasColumnName("user_id").IsRequired(false);
            errorLogBuilder.Property(log => log.MessageName).HasColumnName("message_name").IsRequired();
            errorLogBuilder.Property(log => log.Text).HasColumnName("text").IsRequired();
            errorLogBuilder.Property(log => log.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        }
    }
}
