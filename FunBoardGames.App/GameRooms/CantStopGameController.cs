
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
        SortedDictionary<int, int> whiteConePositions = new();
        int currentPlayerIndex = 0;
        HashSet<int> FinishedColumns = new();

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

        public SortedDictionary<int, int> PlaceWhiteCone(PlaceWhiteConeRequestMessage request)
        {
            int sumDice1 = diceValues[request.DiceIndex1] + diceValues[request.DiceIndex2];
            int sumDice2 = diceValues.Sum() - sumDice1;
            SortedDictionary<int, int> result = new();

            if (sumDice1 == sumDice2)
            {
                var move = GetPosibleMove(sumDice1, players[currentPlayerIndex]);

                if(move == null || move.Value.pos >= columnData[sumDice1] - 2)
                    return result;

                whiteConePositions[sumDice1] = move.Value.pos + 1;
                return new SortedDictionary<int, int>() { {sumDice1, move.Value.pos + 1 } };
            }

            SortedDictionary<int, (int pos, int cone)?> possibleMoves = new();

            (int pos, int cone)? move1 = GetPosibleMove(sumDice1, players[currentPlayerIndex]);
            (int pos, int cone)? move2 = GetPosibleMove(sumDice2, players[currentPlayerIndex]);
            possibleMoves[sumDice1] = move1;
            possibleMoves[sumDice2] = move2;

            if (move1 == null && move2 == null)
                return result;


            int newWhiteCones = (move1 != null ? move1.Value.cone : 0) + (move2 != null ? move2.Value.cone : 0);

            bool mustSelectMoves = newWhiteCones + whiteConePositions.Count() > 3;

            if (mustSelectMoves)
            {
                if(possibleMoves[request.Selectedcolumn.Value] != null)
                {
                    whiteConePositions[request.Selectedcolumn.Value] = possibleMoves[request.Selectedcolumn.Value].Value.pos;
                    result[request.Selectedcolumn.Value] = possibleMoves[request.Selectedcolumn.Value].Value.pos;
                    return result;
                }
            }
            
            if(move1 != null)
            {
                whiteConePositions[sumDice1] = move1.Value.pos;
                result[sumDice1] = move1.Value.pos;
            }

            if(move2 != null)
            {
                whiteConePositions[sumDice2] = move2.Value.pos;
                result[sumDice2] = move2.Value.pos;
            }

            return result;
        }

        (int pos, int cone)? GetPosibleMove(int columnNumber, CantStopGamePlayer player)
        {
            if (FinishedColumns.Contains(columnNumber))
                return null;

            int whiteConePos = -1;
            int newCone = 0;

            if (whiteConePositions.TryGetValue(columnNumber, out whiteConePos) == false) // new white cone required
            {
                if (whiteConePositions.Count() >= 3) // Check if we run out of white cones
                    return null;

                newCone = 1;
                if (player.ConePositions.TryGetValue(columnNumber, out whiteConePos) == false)
                    whiteConePos = -1;
            }

            if (whiteConePos == -1)
                return (0, 1);

            if (whiteConePos >= columnData[columnNumber] - 1) // the cone is at 1 cell to the top
                return null;

            return (whiteConePos + 1, newCone);
        }
    }
}
