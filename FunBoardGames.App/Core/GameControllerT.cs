
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public abstract class GameControllerT<TPlayer, TGame>(uint id, TGame game, Func<Profile, TPlayer> createPlayer) 
        : GameController(id) where TPlayer : BoardGamePlayer where TGame : BoardGameData
    {
        protected readonly List<TPlayer> players = [];
        protected readonly TGame game = game;
        readonly Func<Profile, TPlayer> createPlayer = createPlayer;

        public override int GameId => game.Id;
        public override BoardGameType GameType => game.GameType;
        public override int PlayerCount => players.Count;
        public override bool IsOpen => IsStarted == false && players.Count < game.PlayerCount;
        protected bool IsStarted { get; private set; }

        protected abstract void BeginGame();

        public override bool AddPlayer(Profile profile)
        {
            if(players.Exists(p => p.Profile.UserId == profile.UserId))
                return false;

            players.Add(createPlayer(profile));

            if (IsOpen == false && IsStarted == false)
            {
                IsStarted = true;
                BeginGame();
            }

            return true;
        }

        public override bool RemovePlayer(int userId)
        {
            var player = players.FirstOrDefault(player => player.Profile.UserId == userId);
            return player != null && players.Remove(player);
        }
    }
}
