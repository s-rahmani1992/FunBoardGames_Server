using FunBoardGames.App.Core;
using FunBoardGames.App.Lobby;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.SignalR.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

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
            var lobbyService = _provider.GetRequiredService<MatchMakingService>();
            var profile = Context.Items[ContextNames.Profile] as Profile;
            GameController gameController = await lobbyService.JoinGame(joinGameMsg.GameId, profile);

            Context.Items[ContextNames.Room] = gameController;
            await Groups.AddToGroupAsync(Context.ConnectionId, gameController.GroupKey);

            await Clients.OthersInGroup(gameController.GroupKey).SendAsync(LobbyMessageNames.PlayerJoinRoom, new PlayerJoinRoomResponseMessage()
            {
                NewPlayer = new PlayerInfoDTO()
                {
                    UserProfile = new UserProfileDTO
                    {
                        UserId = profile.UserId,
                        PlayerName = profile.PlayerName,
                        Avatar = profile.Avatar,
                    },
                },
            });

            await Clients.Caller.SendAsync(LobbyMessageNames.JoinRoom, new JoinRoomResponseMessage
            {
                RoomId = gameController.RoomId,
                JoinedPlayers = [.. gameController.GetPlayers()],
            });

            await Task.Delay(1000); // TODO: Delay to ensure the player has time to receive the JoinRoom message before sending AllPlayersReady, fix it later

            if (gameController.IsOpen == false)
            {
                await Clients.Group(gameController.GroupKey).SendAsync(LobbyMessageNames.AllPlayersReady);
            }
        }

        [HubMethodName(LobbyMessageNames.PlayerLeave)]
        public async Task LeaveRoom()
        {
            await LeaveRoomInternal();
        }

        [HubMethodName(LobbyMessageNames.JoinStraightGame)]
        public async Task JoinStraightGame(JoinStraightGameRequestMessage joinStraightGameMsg)
        {
            var lobbyService = _provider.GetRequiredService<MatchMakingService>();
            uint? gameId = await lobbyService.FindGameId(joinStraightGameMsg.Game);

            if (gameId == null)
                return;

            await JoinGame(new JoinGameRequestMessage { GameId = gameId.Value });
        }

        async Task LeaveRoomInternal()
        {
            if (Context.Items[ContextNames.Room] != null)
            {
                var profile = Context.Items[ContextNames.Profile] as Profile;

                var gameController = Context.Items[ContextNames.Room] as GameController;
                gameController.RemovePlayer(profile.UserId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameController.GroupKey);

                await Clients.Group(gameController.GroupKey).SendAsync(LobbyMessageNames.PlayerLeave, new PlayerLeaveRoomResponseMessage()
                {
                    UserId = profile.UserId,
                });

                await Clients.Caller.SendAsync(LobbyMessageNames.PlayerLeave, new PlayerLeaveRoomResponseMessage()
                {
                    UserId = profile.UserId,
                });

                if (gameController.PlayerCount == 0)
                {
                    var lobbyService = _provider.GetRequiredService<MatchMakingService>();
                    lobbyService.RemoveGame(gameController);
                }
            }
        }
    }
}
