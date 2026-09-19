
using FunBoardGames.Database.Entities;
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
        public abstract uint GameId {  get; }
        public abstract bool IsOpen { get; }


        public abstract bool AddPlayer(Profile profile);
        public abstract bool RemovePlayer(int UserId);
        public abstract bool SetPlayerLoaded(int UserId);
        public abstract IEnumerable<PlayerInfoDTO> GetPlayers();

        public virtual void OnRemoved() { }

        public uint RoomId { get; private set; }
        public string GroupKey { get; protected set; }
    }
}
