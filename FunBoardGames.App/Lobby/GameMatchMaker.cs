using FunBoardGames.App.Core;
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Lobby
{
    public abstract class GameMatchMaker
    {
        public abstract Task<GameController?> JoinGame(Profile profile, Func<GameController, Task> onJoined);

        public abstract void RemoveGame(GameController gameController);
    }
}
