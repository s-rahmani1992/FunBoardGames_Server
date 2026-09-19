using FunBoardGames.App.Core;
using FunBoardGames.Database.Entities;

namespace FunBoardGames.App.CantStopGame
{
    public class CantStopGamePlayer : BoardGamePlayer
    {
        public CantStopGamePlayer(Profile profile) : base(profile)
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
