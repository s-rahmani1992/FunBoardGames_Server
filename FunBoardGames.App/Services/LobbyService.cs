using FunBoardGames.App.GameRooms;
using FunBoardGames.Network.SignalR.Shared;
using System.Collections.Concurrent;

namespace FunBoardGames.App.Services
{
    public class LobbyService
    {
        ConcurrentDictionary<int, SETGameController> SETGames = new();
        ConcurrentDictionary<int, CantStopGameController> CantStopGames = new();

        int roomId = 0;

        public GameController CreateGame(BoardGameType gameType, string roomName, bool straightMode = false)
        {
            int newGameId = straightMode ? -1 : Interlocked.Increment(ref roomId);
            GameController gameController;

            switch (gameType)
            {
                case BoardGameType.SET:
                    var setGame = new SETGameController(roomName, newGameId);
                    gameController = setGame;
                    SETGames[newGameId] = setGame;
                    break;
                case BoardGameType.CantStop:
                    var cantStopGame = new CantStopGameController(roomName, newGameId);
                    gameController = cantStopGame;
                    CantStopGames[newGameId] = cantStopGame;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), $"Unsupported game type: {gameType}");
            }

            return gameController;
        }

        public GameController? GetGame(BoardGameType gameType, int roomId)
        {
            switch (gameType)
            {
                case BoardGameType.SET:
                    SETGames.TryGetValue(roomId, out SETGameController gamesetController);
                    return gamesetController;
                case BoardGameType.CantStop:
                    CantStopGames.TryGetValue(roomId, out CantStopGameController cantStopGameController);
                    return cantStopGameController;
            }

            return null;
        }

        public IEnumerable<GameController> GetGames(BoardGameType gameType)
        {
            switch(gameType)
            {
                case BoardGameType.SET:
                    return SETGames.Values;
                case BoardGameType.CantStop:
                    return CantStopGames.Values;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), $"Unsupported game type: {gameType}");
            }
        }

        public void RemoveGame(GameController gameController) 
        {
            SETGames.Remove(gameController.RoomId, out _);
            CantStopGames.Remove(gameController.RoomId, out _ );
        }
    }
}
