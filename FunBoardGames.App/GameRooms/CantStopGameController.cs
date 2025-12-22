
using FunBoardGames.App.Services;

namespace FunBoardGames.App.GameRooms
{
    public class CantStopGameController : GameController
    {
        public override int PlayerCount => throw new NotImplementedException();

        public CantStopGameController(string roomName, int id) : base(roomName, id)
        {
        }

        public override bool AddPlayer(string connectionId, string playerName)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Profile> GetPlayers()
        {
            throw new NotImplementedException();
        }

        public override RoomInfo GetInfo()
        {
            return new RoomInfo()
            {
                GameType = Messages.BoardGame.CantStop,
                Id = RoomId,
                MaxPlayers = 4,
                PlayerCount = 1,
                Name = RoomName,
            };
        }

        public override bool RemovePlayer(string connectionId)
        {
            throw new NotImplementedException();
        }
    }
}
