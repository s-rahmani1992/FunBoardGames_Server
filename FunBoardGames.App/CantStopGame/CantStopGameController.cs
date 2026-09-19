
using FunBoardGames.App.Core;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.CantStop;

namespace FunBoardGames.App.CantStopGame
{
    public class CantStopGameController : GameControllerT<CantStopGamePlayer, CantStopGameData>
    {
        SortedDictionary<int, int> whiteConePositions = new();
        int currentPlayerIndex = 0;
        public HashSet<int> FinishedColumns = new();

        int[] diceValues = new int[4];

        public CantStopGameController(uint id, CantStopGameData entity) : base(id, entity, (profile) => new CantStopGamePlayer(profile))
        {
            GroupKey = "Cant_Stop_" + RoomId;
        }

        public int GetCurrentPlayerConnectionId()
        {
            return players[currentPlayerIndex].Profile.UserId;
        }

        public CantStopBoardDTO GetBoardData()
        {
            return new CantStopBoardDTO()
            {
                Columns = game.BoardData,
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

                if (move == null || move.Value.pos >= game.BoardData[sumDice1] - 1)
                    return result;

                whiteConePositions[sumDice1] = move.Value.pos + 1;
                return new SortedDictionary<int, int>() { { sumDice1, move.Value.pos + 1 } };
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
                if (possibleMoves[request.Selectedcolumn.Value] != null)
                {
                    whiteConePositions[request.Selectedcolumn.Value] = possibleMoves[request.Selectedcolumn.Value].Value.pos;
                    result[request.Selectedcolumn.Value] = possibleMoves[request.Selectedcolumn.Value].Value.pos;
                    return result;
                }
            }

            if (move1 != null)
            {
                whiteConePositions[sumDice1] = move1.Value.pos;
                result[sumDice1] = move1.Value.pos;
            }

            if (move2 != null)
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

            if (whiteConePos >= game.BoardData[columnNumber] - 1) // the cone is at 1 cell to the top
                return null;

            return (whiteConePos + 1, newCone);
        }

        public int UpdatePlayerCone()
        {
            var player = players[currentPlayerIndex];
            foreach (var whiteCone in whiteConePositions)
            {
                int columnNumber = whiteCone.Key;
                int newPos = whiteCone.Value;

                player.ConePositions[columnNumber] = newPos;
                if (newPos >= game.BoardData[columnNumber] - 1)
                {
                    player.AddScore(1);
                    FinishedColumns.Add(columnNumber);
                }
            }

            whiteConePositions.Clear();
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count();
            return players[currentPlayerIndex].Profile.UserId;
        }

        public bool IsBusted()
        {
            var player = players[currentPlayerIndex];

            int sumDice1 = diceValues[0] + diceValues[1];
            int sumDice2 = diceValues[2] + diceValues[3];
            bool sum1Valid = GetPosibleMove(sumDice1, player) != null;
            bool sum2Valid = GetPosibleMove(sumDice2, player) != null;

            if (sum1Valid || sum2Valid)
                return false;

            sumDice1 = diceValues[0] + diceValues[2];
            sumDice2 = diceValues[1] + diceValues[3];
            sum1Valid = GetPosibleMove(sumDice1, player) != null;
            sum2Valid = GetPosibleMove(sumDice2, player) != null;

            if (sum1Valid || sum2Valid)
                return false;

            sumDice1 = diceValues[0] + diceValues[3];
            sumDice2 = diceValues[1] + diceValues[2];
            sum1Valid = GetPosibleMove(sumDice1, player) != null;
            sum2Valid = GetPosibleMove(sumDice2, player) != null;

            if (sum1Valid || sum2Valid)
                return false;


            return true;
        }

        public int EndRound()
        {
            whiteConePositions.Clear();
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count();
            return players[currentPlayerIndex].Profile.UserId;
        }

        public CantStopGamePlayer GetPlayerByConnectionId(int userId)
        {
            return players.FirstOrDefault(player => player.Profile.UserId == userId);
        }

        public bool IsFinished()
        {
            return players.Any(player => player.Score >= 3);
        }

        public List<PlayerScoreDTO> GetPlayerScores()
        {
            return players.OrderByDescending(player => player.Score).Select(player => new PlayerScoreDTO
            {
                UserId = player.Profile.UserId,
                Score = player.Score,
            }).ToList();
        }
    }
}
