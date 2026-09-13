using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunBoardGames.Database.Configurations
{
    public class BoardGameConfiguration : IEntityTypeConfiguration<BoardGameData>
    {
        public void Configure(EntityTypeBuilder<BoardGameData> boardGamebuilder)
        {
            boardGamebuilder.ToTable("Board_Games");
            boardGamebuilder.UseTptMappingStrategy();
            boardGamebuilder.HasKey(game => game.Id);
            boardGamebuilder.Property(game=>game.Id).HasColumnName("id");
            boardGamebuilder.Property(game=>game.Name).HasColumnName("name").IsRequired().HasDefaultValue("game");
            boardGamebuilder.Property(game => game.GameType).HasColumnName("game_type").IsRequired().HasDefaultValue(BoardGameType.SET);
            boardGamebuilder.Property(game => game.PlayerCount).HasColumnName("player_count").IsRequired().HasDefaultValue(2);
            boardGamebuilder.Property(game => game.Created_At).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            boardGamebuilder.Property(game => game.Deleted_At).HasColumnName("deleted_at").IsRequired(false);
        }
    }
}
