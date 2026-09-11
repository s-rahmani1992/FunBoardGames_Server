
using System;
using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared
{
    public static class LobbyMessageNames
    {
        public const string JoinRoom = "JoinRoom";
        public const string PlayerJoinRoom = "PlayerJoinRoom";
        public const string PlayerLeave = "PlayerLeave";
        public const string PlayerReady = "PlayerReady";
        public const string AllPlayersReady = "AllPlayersReady";
        public const string JoinStraightGame = "JoinStraightGame";
        public const string JoinGame = "JoinGame";
    }

    public enum BoardGameType
    {
        SET,
        CantStop,
    }

    public class RoomInfoDTO
    {
        public uint Id { get; set; }
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

    // Superseded by JoinGame (matchmaking): creating a named room is no longer
    // supported, but the message is kept so old clients still deserialize.
    [Obsolete("Creating a named room is retired in favor of matchmaking (JoinGame). RoomName is no longer honored.")]
    public class CreateRoomRequestMessage
    {
        public BoardGameType Game { get; set; }
        public string RoomName { get; set; } = string.Empty;
    }

    // Superseded by JoinGame (matchmaking): joining a specific room by id is
    // no longer the primary flow.
    [Obsolete("Joining a specific room by id is retired in favor of matchmaking (JoinGame).")]
    public class JoinRoomRequestMessage
    {
        public BoardGameType Game { get; set; }
        public int RoomId { get; set; }
    }

    public class JoinRoomResponseMessage
    {
        public BoardGameType Game { get; set; }
        public uint RoomId { get; set; }
        public string RoomName { get; set; }
        public List<PlayerInfoDTO> JoinedPlayers { get; set; }
    }

    [Obsolete("Listing all rooms is retired in favor of matchmaking (JoinGame).")]
    public class GetRoomListRequestMessage
    {
        public BoardGameType Game { get; set; }
    }

    [Obsolete("Listing all rooms is retired in favor of matchmaking (JoinGame).")]
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

    public class JoinStraightGameRequestMessage
    {
        public BoardGameType Game { get; set; }
    }

    public class JoinGameRequestMessage
    {
        public BoardGameType Game { get; set; }
    }
}
