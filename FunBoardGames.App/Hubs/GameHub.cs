using FunBoardGames.App.GameRooms;
using FunBoardGames.App.Services;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.CantStop;
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

        [HubMethodName(LobbyMessageNames.JoinStraightGame)]
        public async Task JoinStraightGame(JoinStraightGameRequestMessage joinStraightGameMsg)
        {
            var lobbyService = _provider.GetRequiredService<LobbyService>();
            GameController? gameController = lobbyService.GetGame(joinStraightGameMsg.Game, -1);

            gameController ??= lobbyService.CreateGame(joinStraightGameMsg.Game, "Quick Game _ " + joinStraightGameMsg.Game, true);

            gameController.AddPlayer(Context.ConnectionId, Context.Items["name"] as string);
            Context.Items["room"] = gameController;
            gameController.ChangeReady(Context.ConnectionId);
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
                    IsReady = true,
                },
            });

            await Clients.Caller.SendAsync(LobbyMessageNames.JoinRoom, new JoinRoomResponseMessage
            {
                Game = joinStraightGameMsg.Game,
                RoomName = gameController.RoomName,
                RoomId = gameController.RoomId,
                JoinedPlayers = [.. gameController.GetPlayers()],
            });

            if(gameController.PlayerCount == 2 && gameController.AllPlayersReady)
            {
                await Task.Delay(1000);
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

            if (isCorrect)
            {
                await Task.Delay(5000);

                if(setController.HasEnoughCards == false)
                {
                    var newCards = setController.DestributeCards(3);

                    await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.DistributeCards, new DistributeNewCardsMessage
                    {
                        NewCards = newCards,
                    });
                }
                else if (setController.CheckAnySETOnTable() == false)
                {
                    await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.GameEnded, new GameEndedMessage
                    {
                        FinalScores = setController.GetFinalResults(),
                    });

                    var lobbyService = _provider.GetRequiredService<LobbyService>();
                    lobbyService.RemoveGame(setController);
                }
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

        #region Cant Stop

        [HubMethodName(CantStopGameMessageNames.GameLoaded)]
        public async Task SignalCantStopGameLoaded()
        {
            CantStopGameController cantStopController = Context.Items["room"] as CantStopGameController;

            bool allPlayersLoaded = cantStopController.SetGameLoaded(Context.ConnectionId);

            if (allPlayersLoaded)
            {
                var boardData = cantStopController.GetBoardData();
                await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.SendGameData, new GameDataMessage
                {
                    BoardData = boardData,
                    StartPlayerConnectionId = cantStopController.GetCurrentPlayerConnectionId(),
                });
            }
        }

        [HubMethodName(CantStopGameMessageNames.RollDice)]
        public async Task RollDice()
        {
            CantStopGameController cantStopController = Context.Items["room"] as CantStopGameController;
            if (cantStopController.GetCurrentPlayerConnectionId() != Context.ConnectionId)
                return;

            var diceValues = cantStopController.RollDice();
            await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.RollDice, new RollDiceMessage
            {
                diceValues = diceValues,
                IsBusted = cantStopController.IsBusted(),
            });
        }

        [HubMethodName(CantStopGameMessageNames.PlaceWhiteCone)]
        public async Task PlaceWhiteCone(PlaceWhiteConeRequestMessage placeConeMsg)
        {
            CantStopGameController cantStopController = Context.Items["room"] as CantStopGameController;
            if (cantStopController.GetCurrentPlayerConnectionId() != Context.ConnectionId)
                return;
            var moveResult = cantStopController.PlaceWhiteCone(placeConeMsg);
            if (moveResult.Count() > 0)
            {
                await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.PlaceWhiteCone, new PlaceWhiteConeResponseMessage
                {
                    DiceIndex1 = placeConeMsg.DiceIndex1,
                    DiceIndex2 = placeConeMsg.DiceIndex2,
                    PlayerConnectionId = Context.ConnectionId,
                    UpdatedColumns = moveResult,
                });
            }
        }

        [HubMethodName(CantStopGameMessageNames.PlayRound)]
        public async Task PlayRound(PlaceWhiteConeRequestMessage placeConeMsg)
        {
            CantStopGameController cantStopController = Context.Items["room"] as CantStopGameController;
            if (cantStopController.GetCurrentPlayerConnectionId() != Context.ConnectionId)
                return;
            var moveResult = cantStopController.PlaceWhiteCone(placeConeMsg);
            var nextId = cantStopController.UpdatePlayerCone();
            var player = cantStopController.GetPlayerByConnectionId(Context.ConnectionId);
            await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.PlayRound, new PlayRoundResponseMessage
            {
                DiceIndex1 = placeConeMsg.DiceIndex1,
                DiceIndex2 = placeConeMsg.DiceIndex2,
                PlayerConnectionId = Context.ConnectionId,
                UpdatedColumns = moveResult,
                NextPlayerConnectionId = nextId,
                FinalScore = player.Score,
                FinishedColumns = cantStopController.FinishedColumns,
                playerCones = player.ConePositions,
            });

            if(cantStopController.IsFinished())
            {
                await Task.Delay(1000);
                await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.GameFinished, new GameFinishedResponseMessage
                {
                    PlayerScores = cantStopController.GetPlayerScores(),
                });
                var lobbyService = _provider.GetRequiredService<LobbyService>();
                lobbyService.RemoveGame(cantStopController);
            }
        }

        [HubMethodName(CantStopGameMessageNames.EndRound)]
        public async Task EndRound()
        {
            CantStopGameController cantStopController = Context.Items["room"] as CantStopGameController;
            if (cantStopController.GetCurrentPlayerConnectionId() != Context.ConnectionId)
                return;

            var nextId = cantStopController.EndRound();
            await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.EndRound, new EndRoundResponseMessage
            {
                PlayerConnectionId = Context.ConnectionId,
                NextPlayerConnectionId = nextId,
            });
        }

        #endregion
    }
}
