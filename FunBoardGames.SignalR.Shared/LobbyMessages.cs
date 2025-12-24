
using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared
{
    public static class LobbyMessageNames
    {
        public const string CreateRoom = "CreateRoom";
        public const string JoinRoom = "JoinRoom";
        public const string PlayerJoinRoom = "PlayerJoinRoom";
        public const string GetRoomList = "GetRoomList";
        public const string PlayerLeave = "PlayerLeave";
        public const string PlayerReady = "PlayerReady";
        public const string AllPlayersReady = "AllPlayersReady";
    }

    public enum BoardGameType
    {
        SET,
        CantStop,
    }

    public class RoomInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BoardGameType GameType { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
    }

    public class PlayerInfoDTO
    {
        public UserProfileDTO UserProfile { get; set; }
        public bool IsReady { get; set; }
    }

    public class CreateRoomRequestMessage
    {
        public BoardGameType Game { get; set; }
        public string RoomName { get; set; } = string.Empty;
    }

    public class JoinRoomRequestMessage
    {
        public BoardGameType Game { get; set; }
        public int RoomId { get; set; }
    }

    public class JoinRoomResponseMessage
    {
        public BoardGameType Game { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public List<PlayerInfoDTO> JoinedPlayers { get; set; }
    }

    public class GetRoomListRequestMessage
    {
        public BoardGameType Game { get; set; }
    }

    public class GetRoomListResponseMessage
    {
        public List<RoomInfoDTO> Rooms { get; set; }
    }

    public class PlayerJoinRoomResponseMessage
    {
        public PlayerInfoDTO NewPlayer { get; set; }
    }

    public class PlayerLeaveRoomResponseMessage
    {
        public string ConnectionId { get; set; } = string.Empty ;
    }

    public class PlayerReadyResponseMessage 
    { 
        public string ConnectionId { get; set;} = string.Empty ;
    }
}
