
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public abstract class GameController
    {
        public GameController(uint id)
        {
            RoomId = id;
        }

        public abstract int PlayerCount { get; }
        public abstract int RequiredPlayerCount { get; }
        public abstract uint GameId {  get; }

        public abstract bool AddPlayer(string connectionId, string playerName);
        public abstract bool RemovePlayer(string connectionId);
        public abstract void SetPlayerReady(string connectionId);
        public abstract bool SetPlayerLoaded(string connectionId);
        public abstract IEnumerable<PlayerInfoDTO> GetPlayers();
        public abstract RoomInfoDTO GetInfo();

        public uint RoomId { get; private set; }
        public string RoomName { get; private set; } = string.Empty;
        public string GroupKey { get; protected set; }

        public bool IsOpen => PlayerCount < RequiredPlayerCount;
    }
}
