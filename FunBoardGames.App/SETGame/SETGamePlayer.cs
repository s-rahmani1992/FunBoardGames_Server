using FunBoardGames.App.Core;
using FunBoardGames.Database.Entities;

namespace FunBoardGames.App.SETGame
{
    public class SETGamePlayer : BoardGamePlayer
    {
        public SETGamePlayer(Profile profile) : base(profile)
        {
        }

        public int WrongScore { get; set; } = 0;
        public int CorrectScore { get; set; } = 0;
        public bool IsBusted { get; set; } = false;
        public int UsedHintCount { get; set; } = 0;
        public int? LastHintRound { get; set; } = null;

        public int AddWrongScore(int score = 1)
        {
            WrongScore += score;
            return WrongScore;
        }

        public int AddCorrectScore(int score = 1)
        {
            CorrectScore += score;
            return CorrectScore;
        }
    }
}
