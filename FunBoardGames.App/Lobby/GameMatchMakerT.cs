using FunBoardGames.App.Core;
using FunBoardGames.Database.Entities;
using System.Collections.Concurrent;

namespace FunBoardGames.App.Lobby
{
    public class GameMatchMakerT<T> : GameMatchMaker where T : GameController
    {
        readonly ConcurrentDictionary<uint, T> games = new();
        readonly Func<uint, T> createGame;
        uint nextRoomId = 0;
        readonly object matchLock = new();

        /// <param name="createGame">Builds a new game room for the given room id.</param>
        public GameMatchMakerT(Func<uint, T> createGame)
        {
            this.createGame = createGame;
        }

        protected T Join(Profile profile)
        {
            lock (matchLock)
            {
                T game = FindOpenGame() ?? CreateGame();
                game.AddPlayer(profile);
                return game;
            }
        }

        protected void RemoveGame(uint roomId)
        {
            if (games.TryRemove(roomId, out T? game))
                game.OnRemoved();
        }

        T CreateGame()
        {
            uint newRoomId = Interlocked.Increment(ref nextRoomId);
            T game = createGame(newRoomId);
            games[newRoomId] = game;
            return game;
        }

        T? FindOpenGame()
        {
            return games.Values.FirstOrDefault(game => game.IsOpen);
        }

        public override GameController JoinGame(Profile profile)
        {
            return Join(profile);
        }

        public override void RemoveGame(GameController gameController)
        {
            RemoveGame(gameController.RoomId);
        }
    }
}
