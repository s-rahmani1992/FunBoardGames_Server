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
        public const string GameFinished = "CantStop_GameFinished";
    }

    public class CantStopBoardDTO
    {
        public Dictionary<int, int> Columns { get; set; }
    }

    public class PlayerScoreDTO
    {
        public int UserId { get; set; }
        public int Score { get; set; }
    }

    public class GameDataMessage
    {
        public CantStopBoardDTO BoardData { get; set; }
        public int StartPlayerUserId { get; set; }
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
        public int UserId { get; set; }
        public int DiceIndex1 { get; set; }
        public int DiceIndex2 { get; set; }
        public SortedDictionary<int, int> UpdatedColumns { get; set; }
    }

    public class PlayRoundResponseMessage
    {
        public int UserId { get; set; }
        public int DiceIndex1 { get; set; }
        public int DiceIndex2 { get; set; }
        public SortedDictionary<int, int> UpdatedColumns { get; set; }
        public int NextPlayerUserId { get; set; }
        public int FinalScore { get; set; }
        public HashSet<int> FinishedColumns { get; set; }
        public SortedDictionary<int, int> PlayerCones { get; set; }
    }

    public class EndRoundResponseMessage
    {
        public int UserId { get; set; }
        public int NextPlayerUserId { get; set; }
    }

    public class GameFinishedResponseMessage
    {
        public List<PlayerScoreDTO> PlayerScores { get; set; }
    }
}
