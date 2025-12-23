using Microsoft.AspNetCore.SignalR;
using FunBoardGames.App.Messages;
using FunBoardGames.App.Services;
using FunBoardGames.App.GameRooms;
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App
{
    public static class MessageNames
    {
        public const string CreateRoom = "CreateRoom";
        public const string JoinRoom = "JoinRoom";
        public const string PlayerJoinRoom = "PlayerJoinRoom";
        public const string GetRoomList = "GetRoomList";
        public const string PlayerLeave = "PlayerLeave";
    }

    public class GameHub : Hub
    {
        static HashSet<string> connectedUsers = [];

        private readonly IServiceProvider _provider;

        public GameHub(IServiceProvider provider)
        {
            _provider = provider;
        }

        #region Connect And Disconnect

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue("name", out var nameObj) && nameObj is string name)
            {
                connectedUsers.Remove(name);
                await LeaveRoomInternal();
            }

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Authentication

        [HubMethodName(AuthenticationMessageNames.Login)]
        public async Task LoginRequest(LoginRequestMessage loginMsg)
        {
            if(connectedUsers.Add(loginMsg.PlayerName))
            {
                await Clients.Caller.SendAsync(AuthenticationMessageNames.Login, new LoginResponseMessage { 
                    Success = true,
                    Profile = new()
                    {
                        PlayerName = loginMsg.PlayerName,
                        ConnectionId = Context.ConnectionId,
                    },  
                });
                Context.Items["name"] = loginMsg.PlayerName;
            }
            else
            {
                await Clients.Caller.SendAsync(AuthenticationMessageNames.Login, new LoginResponseMessage
                { 
                    Success = false, 
                    ErrorMessage = "Username already taken.", 
                });
            }
        }

        #endregion

        #region Lobby and MatchMaking

        [HubMethodName(MessageNames.CreateRoom)]
        public async Task CreateRoom(CreateRoomRequestMsg createRoomMsg)
        {
            var lobbyService = _provider.GetRequiredService<LobbyService>();

            var gameController = lobbyService.CreateGame(createRoomMsg.Game, createRoomMsg.RoomName);
            
            gameController.AddPlayer(Context.ConnectionId, Context.Items["name"] as string);
            Context.Items["room"] = gameController;
            await Groups.AddToGroupAsync(Context.ConnectionId, gameController.GroupKey);
            await Clients.Caller.SendAsync(MessageNames.JoinRoom, new JoinRoomResponseMsg
            {
                Game = createRoomMsg.Game,
                RoomName = createRoomMsg.RoomName,
                RoomId = gameController.RoomId,
                JoinedPlayers = [.. gameController.GetPlayers()],
            });
        }

        [HubMethodName(MessageNames.JoinRoom)]
        public async Task JoinRoom(JoinRoomRequestMsg joinRoomMsg)
        {
            var lobbyService = _provider.GetRequiredService<LobbyService>();
            GameController? gameController = lobbyService.GetGame(joinRoomMsg.Game, joinRoomMsg.RoomId);

            if (gameController == null)
                return;

            gameController.AddPlayer(Context.ConnectionId, Context.Items["name"] as string);
            Context.Items["room"] = gameController; 
            await Groups.AddToGroupAsync(Context.ConnectionId, gameController.GroupKey);

            await Clients.OthersInGroup(gameController.GroupKey).SendAsync(MessageNames.PlayerJoinRoom, new PlayerJoinRoomResponseMsg()
            {
                NewPlayer = new Profile()
                {
                    ConnectionId = Context.ConnectionId,
                    PlayerName = Context.Items["name"] as string,
                },
            });

            await Clients.Caller.SendAsync(MessageNames.JoinRoom, new JoinRoomResponseMsg
            {
                Game = joinRoomMsg.Game,
                RoomName = gameController.RoomName,
                RoomId = gameController.RoomId,
                JoinedPlayers = [.. gameController.GetPlayers()],
            });
        }

        [HubMethodName(MessageNames.PlayerLeave)]
        public async Task LeaveRoom()
        {
            await LeaveRoomInternal();
        }

        [HubMethodName(MessageNames.GetRoomList)]
        public async Task GetRoomList(GetRoomListRequestMsg getRoomMsg)
        {
            var lobbyService = _provider.GetRequiredService<LobbyService>();

            await Clients.Caller.SendAsync(MessageNames.GetRoomList, new GetRoomListResponseMsg()
            {
                Rooms = lobbyService.GetGames(getRoomMsg.Game).Select(game => game.GetInfo()).ToList(),
            });
        }

        async Task LeaveRoomInternal()
        {
            if (Context.Items["room"] != null)
            {
                var gameController = Context.Items["room"] as GameController;
                gameController.RemovePlayer(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameController.GroupKey);

                await Clients.Group(gameController.GroupKey).SendAsync(MessageNames.PlayerLeave, new PlayerLeaveRoomResponseMsg()
                {
                    ConnectionId = Context.ConnectionId,
                });

                await Clients.Caller.SendAsync(MessageNames.PlayerLeave, new PlayerLeaveRoomResponseMsg()
                {
                    ConnectionId = Context.ConnectionId,
                });

                if(gameController.PlayerCount == 0)
                {
                    var lobbyService = _provider.GetRequiredService<LobbyService>();
                    lobbyService.RemoveGame(gameController);
                }
            }
        }

        #endregion
    }
}
