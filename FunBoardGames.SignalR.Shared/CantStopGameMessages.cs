using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared.CantStop
{
    public static class CantStopGameMessageNames
    {
        public const string GameLoaded = "CantStop_GameLoaded";
        public const string SendGameData = "CantStop_SendGameData";
        public const string RollDice = "CantStop_RollDice";
        public const string PlaceWhiteCone = "CantStop_PlaceWhiteCone";
        public const string PlayRound = "CantStop_PlayRound";
        public const string EndRound = "CantStop_EndRound";
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

    public class RollDiceMessage
    {
        public int[] diceValues { get; set; }
        public bool IsBusted { get; set; }
    }

    public class PlaceWhiteConeRequestMessage
    {
        public int DiceIndex1 { get; set; }
        public int DiceIndex2 { get; set; }
        public int? Selectedcolumn { get; set; }
    }

    public class PlaceWhiteConeResponseMessage
    {
        public string PlayerConnectionId { get; set; }
        public int DiceIndex1 { get; set; }
        public int DiceIndex2 { get; set; }
        public SortedDictionary<int, int> UpdatedColumns { get; set; }
    }

    public class PlayRoundResponseMessage
    {
        public string PlayerConnectionId { get; set; }
        public int DiceIndex1 { get; set; }
        public int DiceIndex2 { get; set; }
        public SortedDictionary<int, int> UpdatedColumns { get; set; }
        public string NextPlayerConnectionId { get; set; }
        public int FinalScore { get; set; }
        public HashSet<int> FinishedColumns { get; set; }
        public SortedDictionary<int, int> playerCones { get; set; }
    }

    public class EndRoundResponseMessage
    {
        public string PlayerConnectionId { get; set; }
        public string NextPlayerConnectionId { get; set; }
    }
}
