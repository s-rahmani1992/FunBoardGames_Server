using FunBoardGames.App.CantStopGame;
using FunBoardGames.App.Core;
using FunBoardGames.App.SETGame;
using FunBoardGames.Database;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace FunBoardGames.App.Lobby
{
    public class MatchMakingService
    {
        public MatchMakingService(IDbContextFactory<FunBoardGamesDbContext> dbContextFactory, IHubContext<GameHub> hubContext)
        {
            _dbContextFactory = dbContextFactory;
            _hubContext = hubContext;
        }

        readonly IDbContextFactory<FunBoardGamesDbContext> _dbContextFactory;
        readonly IHubContext<GameHub> _hubContext;
        readonly ConcurrentDictionary<uint, GameMatchMaker> _activeMatchMakers = new();

        public async Task<GameController> JoinGame(uint gameId, string connectionId, string playerName)
        {
            if (_activeMatchMakers.TryGetValue(gameId, out var matchMaker))
            {
                return matchMaker.JoinGame(connectionId, playerName);
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var gameEntity = await dbContext.GetGameById(gameId);

            GameMatchMaker newMatchMaker = gameEntity switch
            {
                SETGameData setGame => new GameMatchMakerT<SETGameController>(roomId => new SETGameController(roomId, setGame, _hubContext, RemoveGame)),
                CantStopGameData cantStopGame => new GameMatchMakerT<CantStopGameController>(roomId => new CantStopGameController(roomId, cantStopGame)),
                _ => null,
            };

            if (newMatchMaker == null)
                return null;

            newMatchMaker = _activeMatchMakers.GetOrAdd(gameId, newMatchMaker);
            return newMatchMaker.JoinGame(connectionId, playerName);
        }

        public void RemoveGame(GameController gameController)
        {
            if (_activeMatchMakers.TryGetValue(gameController.GameId, out var matchMaker))
            {
                matchMaker.RemoveGame(gameController);
            }
        }

        public async Task<uint?> FindGameId(BoardGameType gameType)
        {
            var entityGameType = ToEntityGameType(gameType);
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var gameEntity = await dbContext.GetGameByType(entityGameType);
            return gameEntity?.Id;
        }

        static BoardGameType ToEntityGameType(BoardGameType gameType) => gameType switch
        {
            BoardGameType.SET => BoardGameType.SET,
            BoardGameType.CantStop => BoardGameType.CantStop,
            _ => throw new ArgumentOutOfRangeException(nameof(gameType)),
        };

        public async Task<List<GameDTO>> GetGames()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var gameEntities = await dbContext.GetAllGames();
            return gameEntities.Select(ToGameDTO).ToList();
        }

        static GameDTO ToGameDTO(BoardGameData entity) => entity switch
        {
            SETGameData setGame => new SETGameDTO
            {
                Id = setGame.Id,
                Name = setGame.Name,
                GameType = BoardGameType.SET,
                PlayerCount = setGame.PlayerCount,
                GuessTime = setGame.PlayerCount,
                RoundTime = setGame.RoundTime,
                WrongLimit = setGame.WrongLimit,
                HintLimit = setGame.HintLimit,
                AttributeCount = setGame.AttributeCount,
            },
            CantStopGameData cantStopGame => new CantStopGameDTO
            {
                Id = cantStopGame.Id,
                Name = cantStopGame.Name,
                GameType = BoardGameType.CantStop,
                PlayerCount = cantStopGame.PlayerCount,
                BoardData = cantStopGame.BoardData,
            },
            _ => throw new NotSupportedException($"Unsupported game entity type: {entity.GetType()}"),
        };
    }
}
