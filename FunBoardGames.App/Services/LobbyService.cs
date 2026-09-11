using FunBoardGames.App.Core;
using FunBoardGames.App.CantStopGame;
using FunBoardGames.App.SETGame;
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Services
{
    public class LobbyService
    {
        readonly GameMatchMaker<SETGameController> setMatchMaker = new("SET");
        readonly GameMatchMaker<CantStopGameController> cantStopMatchMaker = new("CantStop");

        /// <summary>
        /// Finds an open room of the given game type for the player to join,
        /// creating a new one if none is currently open, and adds the player to it.
        /// </summary>
        /// <returns>The game controller the player was added to.</returns>
        public GameController JoinGame(BoardGameType gameType, string connectionId, string playerName)
        {
            switch (gameType)
            {
                case BoardGameType.SET:
                    return setMatchMaker.JoinGame(connectionId, playerName);
                case BoardGameType.CantStop:
                    return cantStopMatchMaker.JoinGame(connectionId, playerName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), $"Unsupported game type: {gameType}");
            }
        }

        public void RemoveGame(GameController gameController)
        {
            switch (gameController)
            {
                case SETGameController setGame:
                    setMatchMaker.RemoveGame(setGame.RoomId);
                    break;
                case CantStopGameController cantStopGame:
                    cantStopMatchMaker.RemoveGame(cantStopGame.RoomId);
                    break;
            }
        }
    }
}
