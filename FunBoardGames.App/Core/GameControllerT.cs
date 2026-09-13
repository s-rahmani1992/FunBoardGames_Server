
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public abstract class GameControllerT<TPlayer, TGame> : GameController where TPlayer : BoardGamePlayer where TGame : BoardGameData
    {
        protected readonly List<TPlayer> players = [];
        protected readonly TGame game;
        readonly Func<string, string, TPlayer> createPlayer;

        protected GameControllerT(uint id, TGame game, Func<string, string, TPlayer> createPlayer) : base(id)
        {
            this.game = game;
            this.createPlayer = createPlayer;
        }

        public override uint GameId => game.Id;

        public override int PlayerCount => players.Count;

        public override bool AddPlayer(string connectionId, string playerName)
        {
            if(players.Exists(p => p.ConnectionId == connectionId))
                return false;

            players.Add(createPlayer(playerName, connectionId));
            return true;
        }

        public override bool RemovePlayer(string connectionId)
        {
            var player = players.FirstOrDefault(player => player.ConnectionId == connectionId);
            return players.Remove(player);
        }

        public override IEnumerable<PlayerInfoDTO> GetPlayers()
        {
            return players.Select(player => new PlayerInfoDTO
            {
                UserProfile = new UserProfileDTO
                {
                    PlayerName = player.Name,
                    ConnectionId = player.ConnectionId,
                },
                IsReady = player.IsReady,
            });
        }

        public override void SetPlayerReady(string connectionId)
        {
            var p = players.FirstOrDefault(player => player.ConnectionId == connectionId);
            p?.SetReady(true);
        }

        public override bool SetPlayerLoaded(string connectionId)
        {
            var player = players.FirstOrDefault(p => p.ConnectionId == connectionId);
            player.SetGameLoaded();

            return players.All(player => player.IsGameLoaded);
        }
    }
}
