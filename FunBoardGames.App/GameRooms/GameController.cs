using FunBoardGames.App.Services;
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.GameRooms
{
    public class Profile
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
    }

    public abstract class GameController
    {
        public int RoomId { get; private set; }
        public string RoomName { get; private set; } = string.Empty;
        public string GroupKey { get; protected set; }
        public abstract int PlayerCount { get; }
        public abstract bool AllPlayersReady { get; }

        public GameController(string roomName, int id) 
        {  
            RoomName = roomName;
            RoomId = id;
        }

        public abstract bool AddPlayer(string connectionId, string playerName);

        public abstract bool RemovePlayer(string connectionId);

        public abstract IEnumerable<PlayerInfoDTO> GetPlayers();

        public abstract RoomInfoDTO GetInfo();

        public abstract void ChangeReady(string connectionId);
    }
}
