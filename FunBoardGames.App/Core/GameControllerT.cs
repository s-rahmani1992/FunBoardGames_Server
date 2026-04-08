
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public abstract class GameControllerT<T> : GameController where T : BoardGamePlayer
    {
        protected List<T> players = [];

        protected GameControllerT(string roomName, int id) : base(roomName, id)
        {
        }

        public override int PlayerCount => players.Count;

        public override bool AllPlayersReady
        {
            get
            {
                int readyCount = players.Where(player => player.IsReady).Count();
                return readyCount == players.Count && players.Count >= MinPlayers;
            }
        }

        public override bool AddPlayer(string connectionId, string playerName)
        {
            if(players.Exists(p => p.ConnectionId == connectionId))
                return false;

            players.Add((T)Activator.CreateInstance(typeof(T), playerName, connectionId));
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
