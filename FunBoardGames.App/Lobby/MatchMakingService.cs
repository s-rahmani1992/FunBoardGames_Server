
using FunBoardGames.App.CantStopGame;
using FunBoardGames.App.Core;
using FunBoardGames.App.SETGame;
using FunBoardGames.Database;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace FunBoardGames.App.Lobby
{
    public class MatchMakingService(IDbContextFactory<FunBoardGamesDbContext> dbContextFactory, IHubContext<GameHub> hubContext)
    {
        readonly IDbContextFactory<FunBoardGamesDbContext> _dbContextFactory = dbContextFactory;
        readonly IHubContext<GameHub> _hubContext = hubContext;
        readonly ConcurrentDictionary<int, GameMatchMaker> _activeMatchMakers = new();

        public async Task<GameController?> JoinGame(int gameId, Profile profile, Func<GameController, Task> onJoined)
        {
            if (_activeMatchMakers.TryGetValue(gameId, out var matchMaker))
            {
                return await matchMaker.JoinGame(profile, onJoined);
            }

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var gameEntity = await dbContext.GetGameById(gameId);

            GameMatchMaker newMatchMaker = gameEntity switch
            {
                SETGameData setGame => new GameMatchMakerT<SETGameController>(roomId => new SETGameController(roomId, setGame, _hubContext, RemoveGame)),
                CantStopGameData cantStopGame => new GameMatchMakerT<CantStopGameController>(roomId => new CantStopGameController(roomId, cantStopGame, _hubContext, RemoveGame)),
                _ => null,
            };

            if (newMatchMaker == null)
                return null;

            newMatchMaker = _activeMatchMakers.GetOrAdd(gameId, newMatchMaker);
            return await newMatchMaker.JoinGame(profile, onJoined);
        }

        public void RemoveGame(GameController gameController)
        {
            if (_activeMatchMakers.TryGetValue(gameController.GameId, out var matchMaker))
            {
                matchMaker.RemoveGame(gameController);
            }
        }

        public async Task<List<BoardGameData>> GetGames()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.GetAllGames();
        }
    }
}
