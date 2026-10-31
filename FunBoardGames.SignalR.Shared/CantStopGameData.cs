using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared
{
    public class CantStopGameData : BoardGameData
    {
        public Dictionary<int, int> BoardData { get; set; } = new Dictionary<int, int>();
        public int MarkerConeCount { get; set; } = 3;
        public int WinConeCount { get; set; } = 3;
        public int RoundTime { get; set; } = 60;
        public int TimeoutLimit { get; set; } = 3;
    }
}
