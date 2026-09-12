namespace FunBoardGames.App.Core
{
    public abstract class GameMatchMaker
    {
        public abstract GameController JoinGame(string connectionId, string playerName);
    
        public abstract void RemoveGame(GameController gameController);
    }
}
