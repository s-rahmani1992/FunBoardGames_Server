namespace FunBoardGames.App.GameRooms
{
    public class SETGamePlayer
    {
        public string Name { get; set; }
        public string ConnectionId { get; set; }
        public SETGamePlayer(string name, string connectionId)
        {
            Name = name;
            ConnectionId = connectionId;
        }
    }
}
