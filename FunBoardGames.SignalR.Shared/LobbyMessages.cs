
using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared
{
    public static class LobbyMessageNames
    {
        public const string JoinGame = "JoinGame";
        public const string PlayerJoinGame = "PlayerJoinRoom";
        public const string PlayerLeave = "PlayerLeave";
        public const string JoinFailed = "JoinFailed";
    }

    public enum BoardGameType
    {
        SET,
        CantStop,
    }

    public class PlayerInfoDTO
    {
        public Profile UserProfile { get; set; }
        public int Index { get; set; }
    }

    public class JoinRoomResponseMessage<TPlayer, TBegin> where TPlayer : PlayerInfoDTO where TBegin : class
    {
        public List<TPlayer> JoinedPlayers { get; set; }
        public TBegin? BeginData { get; set; }
    }

    public class PlayerJoinRoomResponseMessage<TPlayer, TBegin> where TPlayer : PlayerInfoDTO where TBegin : class
    {
        public TPlayer NewPlayer { get; set; }
        public TBegin? BeginData { get; set; }
    }

    public class PlayerLeaveRoomResponseMessage
    {
        public int UserId { get; set; }
    }

    public class JoinFailedMessage
    {
        public string Reason { get; set; }
    }

    public class JoinGameRequestMessage
    {
        public int GameId { get; set; }
    }
}
