using Microsoft.AspNetCore.SignalR;
using FunBoardGames.App.Services;
using FunBoardGames.App.GameRooms;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.SET;

namespace FunBoardGames.App
{
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

        [HubMethodName(LobbyMessageNames.CreateRoom)]
        public async Task CreateRoom(CreateRoomRequestMessage createRoomMsg)
        {
            var lobbyService = _provider.GetRequiredService<LobbyService>();

            var gameController = lobbyService.CreateGame(createRoomMsg.Game, createRoomMsg.RoomName);
            
            gameController.AddPlayer(Context.ConnectionId, Context.Items["name"] as string);
            Context.Items["room"] = gameController;
            await Groups.AddToGroupAsync(Context.ConnectionId, gameController.GroupKey);
            await Clients.Caller.SendAsync(LobbyMessageNames.JoinRoom, new JoinRoomResponseMessage
            {
                Game = createRoomMsg.Game,
                RoomName = createRoomMsg.RoomName,
                RoomId = gameController.RoomId,
                JoinedPlayers = [.. gameController.GetPlayers()],
            });
        }

        [HubMethodName(LobbyMessageNames.JoinRoom)]
        public async Task JoinRoom(JoinRoomRequestMessage joinRoomMsg)
        {
            var lobbyService = _provider.GetRequiredService<LobbyService>();
            GameController? gameController = lobbyService.GetGame(joinRoomMsg.Game, joinRoomMsg.RoomId);

            if (gameController == null)
                return;

            gameController.AddPlayer(Context.ConnectionId, Context.Items["name"] as string);
            Context.Items["room"] = gameController; 
            await Groups.AddToGroupAsync(Context.ConnectionId, gameController.GroupKey);

            await Clients.OthersInGroup(gameController.GroupKey).SendAsync(LobbyMessageNames.PlayerJoinRoom, new PlayerJoinRoomResponseMessage()
            {
                NewPlayer = new PlayerInfoDTO()
                {
                    UserProfile = new UserProfileDTO
                    {
                        ConnectionId = Context.ConnectionId,
                        PlayerName = Context.Items["name"] as string,
                    },
                    IsReady = false,
                },
            });

            await Clients.Caller.SendAsync(LobbyMessageNames.JoinRoom, new JoinRoomResponseMessage
            {
                Game = joinRoomMsg.Game,
                RoomName = gameController.RoomName,
                RoomId = gameController.RoomId,
                JoinedPlayers = [.. gameController.GetPlayers()],
            });
        }

        [HubMethodName(LobbyMessageNames.PlayerLeave)]
        public async Task LeaveRoom()
        {
            await LeaveRoomInternal();
        }

        [HubMethodName(LobbyMessageNames.GetRoomList)]
        public async Task GetRoomList(GetRoomListRequestMessage getRoomMsg)
        {
            var lobbyService = _provider.GetRequiredService<LobbyService>();

            await Clients.Caller.SendAsync(LobbyMessageNames.GetRoomList, new GetRoomListResponseMessage()
            {
                Rooms = lobbyService.GetGames(getRoomMsg.Game).Select(game => game.GetInfo()).ToList(),
            });
        }

        [HubMethodName(LobbyMessageNames.PlayerReady)]
        public async Task PlayerReady()
        {
            GameController gameController = Context.Items["room"] as GameController;
            gameController.ChangeReady(Context.ConnectionId);
            await Clients.Group(gameController.GroupKey).SendAsync(LobbyMessageNames.PlayerReady, new PlayerReadyResponseMessage
            {
                ConnectionId = Context.ConnectionId,
            });

            if (gameController.AllPlayersReady)
            {
                await Clients.Group(gameController.GroupKey).SendAsync(LobbyMessageNames.AllPlayersReady);
            }
        }

        async Task LeaveRoomInternal()
        {
            if (Context.Items["room"] != null)
            {
                var gameController = Context.Items["room"] as GameController;
                gameController.RemovePlayer(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameController.GroupKey);

                await Clients.Group(gameController.GroupKey).SendAsync(LobbyMessageNames.PlayerLeave, new PlayerLeaveRoomResponseMessage()
                {
                    ConnectionId = Context.ConnectionId,
                });

                await Clients.Caller.SendAsync(LobbyMessageNames.PlayerLeave, new PlayerLeaveRoomResponseMessage()
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

        #region SET Game

        [HubMethodName(SETGameMessageNames.GameLoaded)]
        public async Task SignalGameLoaded()
        {
            SETGameController setController = Context.Items["room"] as SETGameController;

            bool allPlayersLoaded = setController.SetGameLoaded(Context.ConnectionId);

            if (allPlayersLoaded)
            {
                setController.PrepareGame();
                var newCards = setController.DestributeCards(12);

                await Task.Delay(4000);
                await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.DistributeCards, new DistributeNewCardsMessage
                {
                    NewCards = newCards,
                });
            }
        }

        #endregion
    }
}
