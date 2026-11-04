
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public enum PlayerState
    {
        Normal,
        Busted,
        Finished,
    }

    public class BoardGamePlayer(Profile profile)
    {
        public Profile Profile { get; protected set; } = profile;
        public int Index { get; internal set; }
        public bool IsConnected { get; private set; } = true;
        public PlayerState State { get; set; } = PlayerState.Normal;
        public bool CanPlay => IsConnected && State == PlayerState.Normal;
        
        internal void Disconnect()
        {
            IsConnected = false;
        }
    }
}
