
using FunBoardGames.Network.SignalR.Shared;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FunBoardGames.SignalR.Shared
{
    public static class UserMessageNames
    {
        public const string GetUserData = "GetUserData";
        public const string ChangePlayerName = "ChangePlayerName";
        public const string ChangePlayerAvatar = "ChangePlayerAvatar";
        public const string ProfileUpdate = "ProfileUpdate";
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
        public int BeginCardCount { get; set; }
        public float GuessTime { get; set; }
        public int RoundTime { get; set; }
        public int WrongLimit { get; set; }
        public int? HintLimit { get; set; }
        public int AttributeCount { get; set; }
    }

    public class CantStopGameDTO : GameDTO
    {
        public Dictionary<int, int> BoardData { get; set; } = new Dictionary<int, int>();
        public int MarkerConeCount { get; set; } = 3;
        public int WinConeCount { get; set; } = 3;
        public int RoundTime { get; set; } = 60;
        public int TimeoutLimit { get; set; } = 3;
    }

    public class GetUserDataResponseMessage
    {
        public List<GameDTO> Games { get; set; } = new List<GameDTO>();
    }
}
