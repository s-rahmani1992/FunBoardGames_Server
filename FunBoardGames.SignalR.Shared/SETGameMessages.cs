
using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared.SET
{
    public static class SETGameMessageNames
    {
        public const string GameLoaded = "SET_GameLoaded";
        public const string DistributeCards = "SET_DistributeCards";
        public const string PlayerGuessStart = "PlayerGuessStart";
        public const string PlayerGuess = "PlayerGuessResult";
    }

    public class SETCardDTO
    {
        public byte Color { get; set; }
        public byte Shape { get; set; }
        public byte CountIndex { get; set; }
        public byte Shading { get; set; }
    }

    public class  DistributeNewCardsMessage
    {
        public List<SETCardDTO> NewCards { get; set; }
    }

    public class PlayerGuessStartMessage
    {
        public string ConnectionId { get; set; }
    }

    public class PlayerCardGuessRequest
    {
        public List<SETCardDTO> GuessedCards { get; set; }
    }

    public class GuessResultResponse
    {
        public string ConnectionId { get; set; }
        public bool GuessedCorrect { get; set; }
        public int CorrectScore { get; set; }
        public int WrongScore { get; set; }
        public List<SETCardDTO>? GuessedCards { get; set; } = null;
    }
}
