
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.GameRooms
{
    public class CantStopGameController : GameController
    {
        public override int PlayerCount => throw new NotImplementedException();

        public override bool AllPlayersReady => throw new NotImplementedException();

        public CantStopGameController(string roomName, int id) : base(roomName, id)
        {
        }

        public override bool AddPlayer(string connectionId, string playerName)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<PlayerInfoDTO> GetPlayers()
        {
            throw new NotImplementedException();
        }

        public override RoomInfoDTO GetInfo()
        {
            return new RoomInfoDTO()
            {
                GameType = BoardGameType.CantStop,
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

        public override void ChangeReady(string connectionId)
        {
            throw new NotImplementedException();
        }
    }
}
