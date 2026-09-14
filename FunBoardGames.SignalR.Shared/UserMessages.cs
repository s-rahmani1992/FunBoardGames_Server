
using FunBoardGames.Network.SignalR.Shared;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FunBoardGames.SignalR.Shared
{
    public static class UserMessageNames
    {
        public const string GetUserData = "GetUserData";
    }

    [JsonConverter(typeof(GameDTOConverter))]
    public abstract class GameDTO
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public BoardGameType GameType { get; set; }
        public uint PlayerCount { get; set; }
    }

    public class SETGameDTO : GameDTO
    {
        public float GuessTime { get; set; }
        public int AttributeCount { get; set; }
    }

    public class CantStopGameDTO : GameDTO
    {
        public Dictionary<int, int> BoardData { get; set; } = new Dictionary<int, int>();
    }

    public class GetUserDataResponseMessage
    {
        public List<GameDTO> Games { get; set; } = new List<GameDTO>();
    }
}
