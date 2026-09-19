using FunBoardGames.Database.Entities;

namespace FunBoardGames.App.Core
{
    public class BoardGamePlayer
    {
        public BoardGamePlayer(Profile  profile)
        {
            Profile = profile;
        }

        public Profile Profile { get; protected set; }

        public bool IsGameLoaded { get; protected set; } = false;

        public void SetGameLoaded()
        {
            IsGameLoaded = true;
        }
    }
}
