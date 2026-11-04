using FunBoardGames.App.Core;
using FunBoardGames.App.Lobby;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App
{
    public partial class GameHub
    {
        [HubMethodName(UserMessageNames.GetUserData)]
        public async Task GetUserData()
        {
            var lobbyService = _provider.GetRequiredService<MatchMakingService>();
            var games = await lobbyService.GetGames();

            await Clients.Caller.SendAsync(UserMessageNames.GetUserData, new GetUserDataResponseMessage
            {
                Games = games,
            });
        }

        [HubMethodName(LobbyMessageNames.JoinGame)]
        public async Task JoinGame(JoinGameRequestMessage joinGameMsg)
        {
            //TODO  This is a workaround for the fact that SignalR does not have a way to automatically remove a user from a group when they disconnect. We should probably move this to OnDisconnectedAsync, but that would require us to keep track of which game the user is in, which we don't currently do.
            await LeaveRoomInternal(notifyCaller: false);

            var lobbyService = _provider.GetRequiredService<MatchMakingService>();
            var profile = Context.Items[ContextNames.Profile] as Profile;

            GameController? gameController = await lobbyService.JoinGame(joinGameMsg.GameId, profile, async controller =>
            {
                Context.Items[ContextNames.Room] = controller;
                await Groups.AddToGroupAsync(Context.ConnectionId, controller.GroupKey);
                await controller.SendJoinMessagesAsync(Clients, profile.UserId);
            });

            if (gameController == null)
            {
                await Clients.Caller.SendAsync(LobbyMessageNames.JoinFailed, new JoinFailedMessage
                {
                    Reason = "Unable to join the game.",
                });
            }
        }

        // Players can only leave on their own while the room is waiting for players. After the game begins, leaving is a disconnection
        [HubMethodName(LobbyMessageNames.PlayerLeave)]
        public async Task LeaveRoom()
        {
            await LeaveRoomInternal(voluntary: true);
        }

        async Task LeaveRoomInternal(bool voluntary = false, bool notifyCaller = true)
        {
            if (Context.Items[ContextNames.Room] is not GameController gameController)
                return;

            var profile = Context.Items[ContextNames.Profile] as Profile;
            var leaveMessage = new PlayerLeaveRoomResponseMessage
            {
                UserId = profile.UserId,
            };

            await gameController.RoomLock.WaitAsync();
            try
            {
                if (voluntary && gameController.IsStarted)
                    return;

                // Leaving twice, e.g. a leave request followed by the disconnection, must not remove the player or the game twice
                Context.Items.Remove(ContextNames.Room);

                if (gameController.RemovePlayer(profile.UserId) == false)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameController.GroupKey);
                    return;
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameController.GroupKey);
                await Clients.Group(gameController.GroupKey).SendAsync(LobbyMessageNames.PlayerLeave, leaveMessage);

                // The caller left on purpose before the game began, so it is the only one that needs to know it is out of the room
                if (notifyCaller && gameController.IsStarted == false)
                    await Clients.Caller.SendAsync(LobbyMessageNames.PlayerLeave, leaveMessage);

                await gameController.OnPlayerLeftAsync(profile.UserId);

                if (gameController.ConnectedPlayerCount == 0)
                {
                    var lobbyService = _provider.GetRequiredService<MatchMakingService>();
                    lobbyService.RemoveGame(gameController);
                }
            }
            finally
            {
                gameController.RoomLock.Release();
            }
        }
    }
}
