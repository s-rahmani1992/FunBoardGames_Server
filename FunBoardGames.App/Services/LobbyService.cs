using FunBoardGames.App.GameRooms;
using FunBoardGames.App.Messages;
using System.Collections.Concurrent;

namespace FunBoardGames.App.Services
{
    public class RoomInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public BoardGame GameType { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
    }

    public class LobbyService
    {
        ConcurrentDictionary<int, SETGameController> SETGames = new();
        ConcurrentDictionary<int, CantStopGameController> CantStopGames = new();

        int roomId = 0;

        public GameController CreateGame(BoardGame gameType, string roomName)
        {
            Interlocked.Increment(ref roomId);
            GameController gameController;

            switch (gameType)
            {
                case BoardGame.SET:
                    var setGame = new SETGameController(roomName, roomId);
                    gameController = setGame;
                    SETGames[roomId] = setGame;
                    break;
                case BoardGame.CantStop:
                    var cantStopGame = new CantStopGameController(roomName, roomId);
                    gameController = cantStopGame;
                    CantStopGames[roomId] = cantStopGame;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), $"Unsupported game type: {gameType}");
            }

            return gameController;
        }

        public GameController? GetGame(BoardGame gameType, int roomId)
        {
            switch (gameType)
            {
                case BoardGame.SET:
                    SETGames.TryGetValue(roomId, out SETGameController gamesetController);
                    return gamesetController;
                case BoardGame.CantStop:
                    CantStopGames.TryGetValue(roomId, out CantStopGameController cantStopGameController);
                    return cantStopGameController;
            }

            return null;
        }

        public IEnumerable<GameController> GetGames(BoardGame gameType)
        {
            switch(gameType)
            {
                case BoardGame.SET:
                    return SETGames.Values;
                case BoardGame.CantStop:
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
