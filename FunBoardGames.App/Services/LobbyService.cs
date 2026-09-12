using FunBoardGames.App.CantStopGame;
using FunBoardGames.App.Core;
using FunBoardGames.App.SETGame;
using FunBoardGames.Database;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.SignalR.Shared;
using Microsoft.EntityFrameworkCore;

namespace FunBoardGames.App.Services
{
    public class LobbyService
    {
        public LobbyService(FunBoardGamesDbContext dbContext, MatchMakerRegistry matchMakerRegistry)
        {
            _dbContext = dbContext;
            _matchMakerRegistry = matchMakerRegistry;
        }

        private readonly FunBoardGamesDbContext _dbContext;
        private readonly MatchMakerRegistry _matchMakerRegistry;

        public async Task<GameController> JoinGame(uint gameId, string connectionId, string playerName)
        {
            if (_matchMakerRegistry.TryGetValue(gameId, out var matchMaker))
            {
                return matchMaker.JoinGame(connectionId, playerName);
            }

            var gameEntity = await _dbContext.Games.FirstOrDefaultAsync(entity => entity.Id == gameId);

            GameMatchMaker newMatchMaker = gameEntity switch
            {
                SETGameEntity setGame => new GameMatchMakerT<SETGameController>(roomId => new SETGameController(roomId, setGame)),
                CantStopGameEntity cantStopGame => new GameMatchMakerT<CantStopGameController>(roomId => new CantStopGameController(roomId, cantStopGame)),
                _ => null,
            };

            if (newMatchMaker == null)
                return null;

            newMatchMaker = _matchMakerRegistry.GetOrAdd(gameId, newMatchMaker);
            return newMatchMaker.JoinGame(connectionId, playerName);
        }

        public void RemoveGame(GameController gameController)
        {
            if (_matchMakerRegistry.TryGetValue(gameController.GameId, out var matchMaker))
            {
                matchMaker.RemoveGame(gameController);
            }
        }

        public async Task<uint?> FindGameId(BoardGameType gameType)
        {
            var entityGameType = ToEntityGameType(gameType);
            var gameEntity = await _dbContext.Games.FirstOrDefaultAsync(entity => entity.GameType == entityGameType);
            return gameEntity?.Id;
        }

        static GameType ToEntityGameType(BoardGameType gameType) => gameType switch
        {
            BoardGameType.SET => GameType.SET,
            BoardGameType.CantStop => GameType.CantStop,
            _ => throw new ArgumentOutOfRangeException(nameof(gameType)),
        };

        public async Task<List<GameDTO>> GetGames()
        {
            var gameEntities = await _dbContext.Games.ToListAsync();
            return gameEntities.Select(ToGameDTO).ToList();
        }

        static GameDTO ToGameDTO(BoardGameEntity entity) => entity switch
        {
            SETGameEntity setGame => new SETGameDTO
            {
                Id = setGame.Id,
                Name = setGame.Name,
                GameType = BoardGameType.SET,
                PlayerCount = setGame.PlayerCount,
                GuessTime = setGame.GuessTime,
            },
            CantStopGameEntity cantStopGame => new CantStopGameDTO
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
