
using FunBoardGames.Network.SignalR.Shared;
using System.Collections.Generic;

namespace FunBoardGames.SignalR.Shared
{
    public static class UserMessageNames
    {
        public const string GetUserData = "GetUserData";
        public const string ChangePlayerName = "ChangePlayerName";
        public const string ChangePlayerAvatar = "ChangePlayerAvatar";
        public const string ProfileUpdate = "ProfileUpdate";
    }

    public class GetUserDataResponseMessage
    {
        public List<BoardGameData> Games { get; set; } = new List<BoardGameData>();
    }
}
