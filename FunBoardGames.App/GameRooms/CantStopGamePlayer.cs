namespace FunBoardGames.App.GameRooms
{
    public class CantStopGamePlayer
    {
        public string Name { get; set; }
        public string ConnectionId { get; set; }
        public bool IsReady { get; set; } = false;
        public bool IsGameLoaded { get; set; }

        public CantStopGamePlayer(string name, string connectionId)
        {
            Name = name;
            ConnectionId = connectionId;
        }

        public void SetReady(bool ready)
        {
            IsReady = ready;
        }

        internal void SetLoaded()
        {
            IsGameLoaded = true;
        }
    }
}
