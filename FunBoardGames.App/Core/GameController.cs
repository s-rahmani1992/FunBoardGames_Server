
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public abstract class GameController
    {
        public GameController(string roomName, int id)
        {
            RoomName = roomName;
            RoomId = id;
        }

        public abstract int PlayerCount { get; }
        public abstract bool AllPlayersReady { get; }

        public abstract bool AddPlayer(string connectionId, string playerName);
        public abstract bool RemovePlayer(string connectionId);
        public abstract void SetPlayerReady(string connectionId);
        public abstract bool SetPlayerLoaded(string connectionId);
        public abstract IEnumerable<PlayerInfoDTO> GetPlayers();
        public abstract RoomInfoDTO GetInfo();

        public int RoomId { get; private set; }
        public string RoomName { get; private set; } = string.Empty;
        public string GroupKey { get; protected set; }
        public int MinPlayers { get; private set; } = 2;
    }
}
