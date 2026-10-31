using FunBoardGames.App.Core;
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.CantStopGame
{
    public class CantStopGamePlayer : BoardGamePlayer
    {
        public CantStopGamePlayer(Profile profile) : base(profile)
        {
        }

        public int Score { get; set; } = 0;
        public int TimeoutCount { get; private set; }

        public SortedDictionary<int, int> ConePositions => conePositions;

        public void AddScore(int score)
        {
            Score += score;
        }

        public void IncreaseTimeout()
        {
            TimeoutCount++;
        }

        readonly SortedDictionary<int, int> conePositions = [];
    }
}
