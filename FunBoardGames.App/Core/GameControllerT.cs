
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public abstract class GameControllerT<TPlayer, TGame>(uint id, TGame game, Func<Profile, TPlayer> createPlayer) 
        : GameController(id) where TPlayer : BoardGamePlayer where TGame : BoardGameData
    {
        protected readonly TPlayer[] players = new TPlayer[game.PlayerCount];
        protected readonly TGame game = game;
        readonly Func<Profile, TPlayer> createPlayer = createPlayer;

        public override int GameId => game.Id;
        public override BoardGameType GameType => game.GameType;
        public override int PlayerCount => players.Count(player => player != null);
        public override int ConnectedPlayerCount => players.Count(player => player != null && player.IsConnected);
        public override bool IsOpen => IsStarted == false && players.Any(player => player == null);
        protected int ActivePlayerCount => players.Count(player => player != null && player.CanPlay);
        protected abstract void BeginGame();

        protected virtual Task OnPlayerDisconnectedAsync(TPlayer player) => Task.CompletedTask;

        public override bool AddPlayer(Profile profile)
        {
            if (IsStarted || players.Any(player => player != null && player.Profile.UserId == profile.UserId))
                return false;

            int seat = Array.FindIndex(players, player => player == null);
            if (seat < 0)
                return false;

            players[seat] = createPlayer(profile);
            players[seat].Index = seat;

            if (IsOpen == false)
            {
                IsStarted = true;
                BeginGame();
            }

            return true;
        }

        public override bool RemovePlayer(int userId)
        {
            int seat = Array.FindIndex(players, player => player != null && player.Profile.UserId == userId);
            if (seat < 0)
                return false;

            if (IsStarted == false)
            {
                players[seat] = null!;
                return true;
            }

            if (players[seat].IsConnected == false)
                return false;

            players[seat].Disconnect();
            return true;
        }

        public override Task OnPlayerLeftAsync(int userId)
        {
            var player = players.FirstOrDefault(player => player != null && player.Profile.UserId == userId);
            return IsStarted && player != null ? OnPlayerDisconnectedAsync(player) : Task.CompletedTask;
        }
    }
}
