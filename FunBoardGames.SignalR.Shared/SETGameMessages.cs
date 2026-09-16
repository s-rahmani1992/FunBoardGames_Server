
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared.SET
{
    public static class SETGameMessageNames
    {
        public const string GameStarted = "SET_GameStarted";
        public const string PlayerGuessStart = "SET_PlayerGuessStart";
        public const string PlayerGuess = "SET_PlayerGuessResult";
    }

    public class SETCardDTO
    {
        public byte Color { get; set; }
        public byte Shape { get; set; }
        public byte CountIndex { get; set; }
        public byte Shading { get; set; }

        public override bool Equals(object card)
        {
            SETCardDTO otherCard = card as SETCardDTO;

            return Color == otherCard.Color &&
                Shape == otherCard.Shape &&
                CountIndex == otherCard.CountIndex &&
                Shading == otherCard.Shading;
            
        }
    }

    public class  SETPlayerResultDTO
    {
        public string ConnectionId { get; set; }
        public int Corrects { get; set; }
        public int Wrongs { get; set; }
    }

    public class GameBeginMessage
    {
        public List<SETCardDTO> NewCards { get; set; }
    }

    public class PlayerGuessStartMessage
    {
        public string ConnectionId { get; set; }
        public DateTimeOffset GuessStartTime { get; set; }
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
        public List<SETCardDTO>? NewCards { get; set; } = null;
        public List<SETPlayerResultDTO>? FinalScores { get; set; } = null;
    }
}
