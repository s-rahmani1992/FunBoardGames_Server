using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App
{
    internal static class ContextNames
    {
        public const string Profile = "profile";
        public const string Room = "room";
    }

    public partial class GameHub : Hub
    {
        private readonly IServiceProvider _provider;

        public GameHub(IServiceProvider provider)
        {
            _provider = provider;
        }

        #region Connect And Disconnect

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await LeaveRoomInternal();

            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
