
namespace FunBoardGames.Database.Entities
{
    public enum GameType
    {
        SET,
        CantStop,
    }

    public class BoardGameData
    {
        public uint Id { get; set; }
        public GameType GameType { get; set; }
        public string Name { get; set; } = string.Empty;
        public uint PlayerCount { get; set; } = 2;
        public DateTimeOffset Created_At { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? Deleted_At { get; set; } = null;
    }
}
