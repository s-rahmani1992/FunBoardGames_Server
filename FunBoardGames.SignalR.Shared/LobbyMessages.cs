
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

    public class PlayerInfoDTO
    {
        public Profile UserProfile { get; set; }
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
        public int GameId { get; set; }
    }
}
