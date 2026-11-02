
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public class BoardGamePlayer(Profile profile)
    {
        public Profile Profile { get; protected set; } = profile;
    }
}
