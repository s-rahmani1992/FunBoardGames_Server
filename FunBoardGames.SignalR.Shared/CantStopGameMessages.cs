using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared.CantStop
{
    public static class CantStopGameMessageNames
    {
        public const string GameLoaded = "CantStop_GameLoaded";
        public const string SendGameData = "CantStop_SendGameData";
        public const string RollDice = "CantStop_RollDice";
        public const string PlaceWhiteCone = "CantStop_PlaceWhiteCone";
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
}
