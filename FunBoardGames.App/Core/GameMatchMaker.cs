using System.Collections.Concurrent;

namespace FunBoardGames.App.Core
{
    public class GameMatchMaker<T> where T : GameController
    {
        readonly ConcurrentDictionary<uint, T> games = new();
        readonly string roomNamePrefix;
        uint nextRoomId = 0;

        readonly object matchLock = new();

        /// <param name="roomNamePrefix">
        /// Used to name rooms this matchmaker creates, e.g. "SET" produces room
        /// names like "Quick Game - SET".
        /// </param>
        public GameMatchMaker(string roomNamePrefix)
        {
            this.roomNamePrefix = roomNamePrefix;
        }

        /// <summary>
        /// Finds an open game room for the player to join, creating a new one when
        /// none is available, adds the player to it and returns that room.
        /// </summary>
        public T JoinGame(string connectionId, string playerName)
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
            T game = (T)Activator.CreateInstance(typeof(T), $"Quick Game - {roomNamePrefix}", newRoomId)!;
            games[newRoomId] = game;
            return game;
        }

        T? FindOpenGame()
        {
            return games.Values.FirstOrDefault(IsOpen);
        }

        static bool IsOpen(T game)
        {
            return game.IsOpen;
        }
    }
}
