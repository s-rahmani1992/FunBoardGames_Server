using FunBoardGames.App.Authentication;
using FunBoardGames.App.CantStopGame;
using FunBoardGames.App.Core;
using FunBoardGames.App.Lobby;
using FunBoardGames.App.SETGame;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.CantStop;
using FunBoardGames.Network.SignalR.Shared.SET;
using FunBoardGames.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App
{
    public partial class GameHub : Hub
    {
        static List<string> connectedUsers = [];

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

        [HubMethodName(AuthenticationMessageNames.SignUp)]
        public async Task SignUpRequest(SignUpRequestMessage signUpMsg)
        {
            var authService = _provider.GetRequiredService<AuthenticationService>();
            var response = await authService.SignUp(signUpMsg);
            await CompleteAuthenticationRequest(AuthenticationMessageNames.SignIn, response);
        }

        [HubMethodName(AuthenticationMessageNames.SignIn)]
        public async Task SignInRequest(SignInRequestMessage signInMsg)
        {
            var authService = _provider.GetRequiredService<AuthenticationService>();
            var response = await authService.SignIn(signInMsg);
            await CompleteAuthenticationRequest(AuthenticationMessageNames.SignIn, response);
        }

        async Task CompleteAuthenticationRequest(string messageName, AuthenticationResponseMessage response)
        {
            if (response.ErrorCode == AuthenticationErrorCode.None && response.ProfileDTO != null)
            {
                response.ProfileDTO.ConnectionId = Context.ConnectionId;
                connectedUsers.Add(response.ProfileDTO.PlayerName);
                Context.Items["name"] = response.ProfileDTO.PlayerName;
                Context.Items["userId"] = response.ProfileDTO.UserId;
            }

            await Clients.Caller.SendAsync(messageName, response);
        }

        #endregion

        #region Lobby and MatchMaking

        [HubMethodName(UserMessageNames.GetUserData)]
        public async Task GetUserData()
        {
            var lobbyService = _provider.GetRequiredService<MatchMakingService>();
            var games = await lobbyService.GetGames();

            await Clients.Caller.SendAsync(UserMessageNames.GetUserData, new GetUserDataResponseMessage
            {
                Games = games,
            });
            int h = 0;
        }

        [HubMethodName(LobbyMessageNames.JoinGame)]
        public async Task JoinGame(JoinGameRequestMessage joinGameMsg)
        {
            var lobbyService = _provider.GetRequiredService<MatchMakingService>();
            GameController gameController = await lobbyService.JoinGame(joinGameMsg.GameId, Context.ConnectionId, Context.Items["name"] as string);

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
                Game = gameController.GetInfo().GameType,
                RoomName = gameController.RoomName,
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

        [HubMethodName(LobbyMessageNames.PlayerReady)]
        public async Task PlayerReady()
        {
            GameController gameController = Context.Items["room"] as GameController;
            gameController.SetPlayerReady(Context.ConnectionId);
            await Clients.Group(gameController.GroupKey).SendAsync(LobbyMessageNames.PlayerReady, new PlayerReadyResponseMessage
            {
                ConnectionId = Context.ConnectionId,
            });
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
                    var lobbyService = _provider.GetRequiredService<MatchMakingService>();
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

            bool allPlayersLoaded = setController.SetPlayerLoaded(Context.ConnectionId);

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

                if(setController.IsDeckEmpty == false)
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

                    var lobbyService = _provider.GetRequiredService<MatchMakingService>();
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

            bool allPlayersLoaded = cantStopController.SetPlayerLoaded(Context.ConnectionId);

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
                var lobbyService = _provider.GetRequiredService<MatchMakingService>();
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
