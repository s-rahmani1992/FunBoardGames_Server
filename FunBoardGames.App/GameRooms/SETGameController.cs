
using FunBoardGames.App.Services;

namespace FunBoardGames.App.GameRooms
{
    public class SETGameController : GameController
    {
        List<SETGamePlayer> players = [];

        public SETGameController(string roomName, int id) : base(roomName, id)
        {
            GroupKey = "SET_" + RoomId;
        }

        public override int PlayerCount => players.Count();

        public override bool AddPlayer(string connectionId, string playerName)
        {
            players.Add(new SETGamePlayer(playerName, connectionId));
            return true;
        }

        public override RoomInfo GetInfo()
        {
            return new RoomInfo()
            {
                GameType = Messages.BoardGame.SET,
                Id = RoomId,
                MaxPlayers = 4,
                PlayerCount = players.Count(),
                Name = RoomName,
            };
        }

        public override IEnumerable<Profile> GetPlayers()
        {
            return players.Select(player => new Profile
            {
                PlayerName = player.Name,
                ConnectionId = player.ConnectionId,
            });
        }

        public override bool RemovePlayer(string connectionId)
        {
            var player = players.FirstOrDefault(player=>player.ConnectionId == connectionId);
            return players.Remove(player);
        }
    }
}
