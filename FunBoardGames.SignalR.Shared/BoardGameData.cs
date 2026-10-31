using System;
using System.Text.Json.Serialization;

namespace FunBoardGames.Network.SignalR.Shared
{
    [JsonConverter(typeof(BoardGameConverter))]
    public class BoardGameData
    {
        public int Id { get; set; }
        public BoardGameType GameType { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlayerCount { get; set; } = 2;

        [JsonIgnore]
        public DateTimeOffset Created_At { get; set; } = DateTimeOffset.UtcNow;

        [JsonIgnore]
        public DateTimeOffset? Deleted_At { get; set; } = null;
    }
}
