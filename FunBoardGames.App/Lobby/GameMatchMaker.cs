using FunBoardGames.App.Core;

namespace FunBoardGames.App.Lobby
{
    public abstract class GameMatchMaker
    {
        public abstract GameController JoinGame(string connectionId, string playerName);
    
        public abstract void RemoveGame(GameController gameController);
    }
}
