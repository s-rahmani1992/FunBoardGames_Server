using FunBoardGames.App.Core;

namespace FunBoardGames.App.SETGame
{
    public class SETGamePlayer : BoardGamePlayer
    {
        public SETGamePlayer(string name, string connectionId) : base(name, connectionId)
        {
        }

        public int WrongScore { get; set; } = 0;
        public int CorrectScore { get; set; } = 0;
        public bool IsBusted { get; set; } = false;

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
