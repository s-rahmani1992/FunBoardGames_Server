using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared.CantStop
{
    public static class CantStopGameMessageNames
    {
        public const string GameLoaded = "CantStop_GameLoaded";
        public const string SendGameData = "CantStop_SendGameData";
    }

    public class CantStopBoardDTO
    {
        public Dictionary<int, int> Columns { get; set; }
    }

    public class GameDataMessage
    {
        public CantStopBoardDTO BoardData { get; set; }
        public string StartPlayerConnectionId { get; set; }
    }
}
