using FunBoardGames.App.Core;
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Lobby
{
    public abstract class GameMatchMaker
    {
        public abstract GameController JoinGame(Profile profile);
    
        public abstract void RemoveGame(GameController gameController);
    }
}
