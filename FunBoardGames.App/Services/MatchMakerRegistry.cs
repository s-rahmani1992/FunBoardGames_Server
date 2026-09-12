using FunBoardGames.App.Core;
using System.Collections.Concurrent;

namespace FunBoardGames.App.Services
{
    /// <summary>
    /// Holds active <see cref="GameMatchMaker"/> instances for the lifetime of the app.
    /// Registered as a singleton so matchmaking state survives across hub method calls,
    /// unlike <see cref="LobbyService"/> which is scoped per call.
    /// </summary>
    public class MatchMakerRegistry
    {
        readonly ConcurrentDictionary<uint, GameMatchMaker> _activeMatchMakers = new();

        public bool TryGetValue(uint gameId, out GameMatchMaker matchMaker) =>
            _activeMatchMakers.TryGetValue(gameId, out matchMaker);

        public GameMatchMaker GetOrAdd(uint gameId, GameMatchMaker matchMaker) =>
            _activeMatchMakers.GetOrAdd(gameId, matchMaker);
    }
}
