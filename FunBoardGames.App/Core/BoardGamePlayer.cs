namespace FunBoardGames.App.Core
{
    public class BoardGamePlayer
    {
        public BoardGamePlayer(string name, string connectionId)
        {
            Name = name;
            ConnectionId = connectionId;
        }

        public string Name { get; protected set; }
        public string ConnectionId { get; protected set; }
        public bool IsReady { get; protected set; } = false;
        public bool IsGameLoaded { get; protected set; } = false;

        public void SetReady(bool ready)
        {
            IsReady = ready;
        }

        public void SetGameLoaded()
        {
            IsGameLoaded = true;
        }
    }
}
