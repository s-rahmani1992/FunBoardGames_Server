using System.Collections.Concurrent;

namespace FunBoardGames.App.Core
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

        /// <summary>
        /// Finds an open game room for the player to join, creating a new one when
        /// none is available, adds the player to it and returns that room.
        /// </summary>
        public T Join(string connectionId, string playerName)
        {
            lock (matchLock)
            {
                T game = FindOpenGame() ?? CreateGame();
                game.AddPlayer(connectionId, playerName);
                return game;
            }
        }

        /// <summary>Stops tracking the room with the given id (e.g. once it's empty or finished).</summary>
        public void RemoveGame(uint roomId)
        {
            games.TryRemove(roomId, out _);
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

        public override GameController JoinGame(string connectionId, string playerName)
        {
            return Join(connectionId, playerName);
        }

        public override void RemoveGame(GameController gameController)
        {
            RemoveGame(gameController.RoomId);
        }
    }
}
