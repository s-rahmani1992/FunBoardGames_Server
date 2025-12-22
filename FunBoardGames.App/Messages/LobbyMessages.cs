
using FunBoardGames.App.GameRooms;
using FunBoardGames.App.Services;

namespace FunBoardGames.App.Messages
{
    public enum BoardGame
    {
        SET,
        CantStop,
    }

    public class CreateRoomRequestMsg
    {
        public BoardGame Game { get; set; }
        public string RoomName { get; set; } = string.Empty;
    }

    public class JoinRoomRequestMsg
    {
        public BoardGame Game { get; set; }
        public int RoomId { get; set; }
    }

    public class GetRoomListRequestMsg
    {
        public BoardGame Game { get; set; }
    }

    public class JoinRoomResponseMsg 
    { 
        public BoardGame Game { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public List<Profile> JoinedPlayers { get; set; }
    }

    public class PlayerJoinRoomResponseMsg
    {
        public Profile NewPlayer { get; set; }
    }

    public class GetRoomListResponseMsg
    {
        public List<RoomInfo> Rooms { get; set; } = [];
    }

    public class PlayerLeaveRoomResponseMsg
    {
        public string ConnectionId { get; set; }
    }
}
