
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.CantStop;

namespace FunBoardGames.App.GameRooms
{
    public class CantStopGameController : GameController
    {
        static Dictionary<int, int> columnData;

        static CantStopGameController()
        {
            columnData = new Dictionary<int, int>()
            {
                {12, 3},
                {11, 5},
                {10, 6},
                {9, 7},
                {8, 8},
                {7, 8},
                {6, 6},
                {5, 5},
                {4, 5},
                {3, 4},
                {2, 4},
            };
        }

        List<CantStopGamePlayer> players = [];
        int minPlayers = 2;

        int currentPlayerIndex = 0;

        int[] diceValues = new int[4];

        public override int PlayerCount => players.Count();

        public override bool AllPlayersReady
        {
            get
            {
                int readyCount = players.Where(player => player.IsReady).Count();

                return readyCount == players.Count && players.Count >= minPlayers;
            }
        }

        public CantStopGameController(string roomName, int id) : base(roomName, id)
        {
            GroupKey = "Cant_Stop_" + RoomId;
        }

        public override bool AddPlayer(string connectionId, string playerName)
        {
            players.Add(new CantStopGamePlayer(playerName, connectionId));
            return true;
        }

        public override IEnumerable<PlayerInfoDTO> GetPlayers()
        {
            return players.Select(player => new PlayerInfoDTO
            {
                UserProfile = new UserProfileDTO
                {
                    PlayerName = player.Name,
                    ConnectionId = player.ConnectionId,
                },
                IsReady = player.IsReady,
            });
        }

        public override RoomInfoDTO GetInfo()
        {
            return new RoomInfoDTO()
            {
                GameType = BoardGameType.CantStop,
                Id = RoomId,
                MaxPlayers = 4,
                PlayerCount = PlayerCount,
                Name = RoomName,
            };
        }

        public string GetCurrentPlayerConnectionId()
        {
            return players[currentPlayerIndex].ConnectionId;
        }

        public override bool RemovePlayer(string connectionId)
        {
            var player = players.FirstOrDefault(player => player.ConnectionId == connectionId);
            return players.Remove(player);
        }

        public override void ChangeReady(string connectionId)
        {
            var p = players.FirstOrDefault(player => player.ConnectionId == connectionId);
            p?.SetReady(true);
        }

        internal bool SetGameLoaded(string connectionId)
        {
            var player = players.FirstOrDefault(p => p.ConnectionId == connectionId);
            player.SetLoaded();

            return players.All(player => player.IsGameLoaded);
        }

        public CantStopBoardDTO GetBoardData()
        {
            return new CantStopBoardDTO()
            {
                Columns = columnData,
            };
        }

        public int[] RollDice()
        {
            var random = new Random();
            for (int i = 0; i < 4; i++)
            {
                diceValues[i] = random.Next(1, 7);
            }

            return diceValues;
        }
    }
}
