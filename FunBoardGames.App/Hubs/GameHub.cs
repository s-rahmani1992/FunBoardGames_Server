using FunBoardGames.App.GameRooms;
using FunBoardGames.App.Services;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.SET;
using Microsoft.AspNetCore.SignalR;

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

        [HubMethodName(SETGameMessageNames.PlayerGuessStart)]
        public async Task PlayerStartGuess()
        {
            SETGameController setController = Context.Items["room"] as SETGameController;
            await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.PlayerGuessStart, new PlayerGuessStartMessage
            {
                ConnectionId = Context.ConnectionId,
            });

            var player = await setController.StartGuessProcess(Context.ConnectionId, Clients.Group(setController.GroupKey));
        }

        [HubMethodName(SETGameMessageNames.PlayerGuess)]
        public async Task ProcessGuess(PlayerCardGuessRequest cardGuessMsg)
        {
            SETGameController setController = Context.Items["room"] as SETGameController;
            SETGamePlayer player;
            bool isCorrect = setController.ProcessGuess(Context.ConnectionId, cardGuessMsg.GuessedCards, out player);
            await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.PlayerGuess, new GuessResultResponse
            {
                ConnectionId = Context.ConnectionId,
                GuessedCorrect = isCorrect,
                WrongScore = player.WrongScore,
                CorrectScore = player.CorrectScore,
                GuessedCards = cardGuessMsg.GuessedCards,
            });

            if (isCorrect && setController.HasEnoughCards == false)
            {
                await Task.Delay(5000);
                var newCards = setController.DestributeCards(3);

                await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.DistributeCards, new DistributeNewCardsMessage
                {
                    NewCards = newCards,
                });
            }
        }

        [HubMethodName(SETGameMessageNames.PlayerStartCardVote)]
        public async Task PlayerStartVote()
        {
            SETGameController setController = Context.Items["room"] as SETGameController;
            setController.ProcessCardVote(Context.ConnectionId, true);

            await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.PlayerStartCardVote, new PlayerStartedVoteResponse
            {
                ConnectionId = Context.ConnectionId,
            });
        }

        [HubMethodName(SETGameMessageNames.PlayerCardVote)]
        public async Task PlayerVoted(PlayerVoteRequest voteMsg)
        {
            SETGameController setController = Context.Items["room"] as SETGameController;
            bool? result = setController.ProcessCardVote(Context.ConnectionId, voteMsg.Vote);

            await Clients.OthersInGroup(setController.GroupKey).SendAsync(SETGameMessageNames.PlayerCardVote, new PlayerVoteResponse
            {
                ConnectionId = Context.ConnectionId,
                IsVoteYes = voteMsg.Vote,
            });

            if (result.HasValue) 
            {

                await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.CardVoteResult, new VoteResultResponse
                {
                    VotePassed = result.Value,
                });

                if (result.Value)
                {
                    await Task.Delay(1000);
                    var cards = setController.DestributeCards(3);
                    await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.DistributeCards, new DistributeNewCardsMessage
                    {
                        NewCards = cards,
                    });
                }
            }
        }

        #endregion
    }
}
