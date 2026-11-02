using FunBoardGames.App.Core;
using FunBoardGames.Network.SignalR.Shared;
using System.Collections.Concurrent;

namespace FunBoardGames.App.Lobby
{
    public class GameMatchMakerT<T>(Func<uint, T> createGame) : GameMatchMaker where T : GameController
    {
        readonly ConcurrentDictionary<uint, T> games = new();
        readonly Func<uint, T> createGame = createGame;
        uint nextRoomId = 0;
        readonly object matchLock = new();

        protected async Task<T?> Join(Profile profile, Func<GameController, Task> onJoined)
        {
            while (true)
            {
                T game;
                lock (matchLock)
                    game = FindOpenGame() ?? CreateGame();

                await game.RoomLock.WaitAsync();
                try
                {
                    if (game.IsOpen == false || games.ContainsKey(game.RoomId) == false)
                        continue;

                    if (game.AddPlayer(profile) == false)
                        return null;

                    await onJoined(game);
                    return game;
                }
                finally
                {
                    game.RoomLock.Release();
                }
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

        public override async Task<GameController?> JoinGame(Profile profile, Func<GameController, Task> onJoined)
        {
            return await Join(profile, onJoined);
        }

        public override void RemoveGame(GameController gameController)
        {
            RemoveGame(gameController.RoomId);
        }
    }
}
