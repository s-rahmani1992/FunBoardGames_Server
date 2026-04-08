using FunBoardGames.App.Core;

namespace FunBoardGames.App.CantStopGame
{
    public class CantStopGamePlayer : BoardGamePlayer
    {
        public CantStopGamePlayer(string name, string connectionId) : base(name, connectionId)
        {
        }

        public int Score { get; set; } = 0;

        public SortedDictionary<int, int> ConePositions => conePositions;

        public void AddScore(int score)
        {
            Score += score;
        }

        readonly SortedDictionary<int, int> conePositions = [];
    }
}
