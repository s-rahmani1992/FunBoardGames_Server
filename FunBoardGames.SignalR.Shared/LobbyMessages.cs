
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
    }

    public class JoinRoomResponseMessage
    {
        public uint RoomId { get; set; }
        public List<PlayerInfoDTO> JoinedPlayers { get; set; }
    }

    public class PlayerJoinRoomResponseMessage
    {
        public PlayerInfoDTO NewPlayer { get; set; }
    }

    public class PlayerLeaveRoomResponseMessage
    {
        public int UserId { get; set; }
    }

    public class JoinStraightGameRequestMessage
    {
        public BoardGameType Game { get; set; }
    }

    public class JoinGameRequestMessage
    {
        public uint GameId { get; set; }
    }
}
