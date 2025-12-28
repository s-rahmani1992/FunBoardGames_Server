
using FunBoardGames.App.SETGame;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.SET;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App.GameRooms
{
    public class SETGameController : GameController
    {
        private static List<SETCardDTO> SETCardData;

        List<SETGamePlayer> players = [];
        int minPlayers = 2;
        byte[] cards;
        int cardCursor = 0;
        List<SETCardDTO> placedCards = [];

        CancellationTokenSource guessCancelTokenSource;

        const int guessTime = 7000;

        public override int PlayerCount => players.Count();

        public override bool AllPlayersReady
        {
            get
            {
                int readyCount = players.Where(player => player.IsReady).Count();

                return readyCount == players.Count && players.Count >= minPlayers;
            }
        }

        public bool HasEnoughCards => (cardCursor >= 80 || placedCards.Count >= 12);

        static SETGameController()
        {
            SETCardData = new(81);
            for (byte i = 0; i < 3; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    for (byte k = 0; k < 3; k++)
                    {
                        for (byte l = 0; l < 3; l++)
                            SETCardData.Add(new SETCardDTO
                            {
                                Color = i,
                                CountIndex = j,
                                Shape = k,
                                Shading = l,
                            });
                    }
                }
            }
        }

        public SETGameController(string roomName, int id) : base(roomName, id)
        {
            GroupKey = "SET_" + RoomId;
        }

        public override bool AddPlayer(string connectionId, string playerName)
        {
            players.Add(new SETGamePlayer(playerName, connectionId));
            return true;
        }

        public override void ChangeReady(string connectionId)
        {
            var p = players.FirstOrDefault(player => player.ConnectionId ==  connectionId);
            p?.SetReady(true);
        }

        public override RoomInfoDTO GetInfo()
        {
            return new RoomInfoDTO
            {
                GameType = BoardGameType.SET,
                Id = RoomId,
                MaxPlayers = 4,
                PlayerCount = players.Count(),
                Name = RoomName,
            };
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

        public override bool RemovePlayer(string connectionId)
        {
            var player = players.FirstOrDefault(player=>player.ConnectionId == connectionId);
            return players.Remove(player);
        }

        internal bool SetGameLoaded(string connectionId)
        {
            var player = players.FirstOrDefault(p => p.ConnectionId == connectionId);
            player.SetLoaded();

            return players.All(player => player.IsGameLoaded);
        }

        internal void PrepareGame()
        {
            cards = GetRandomByteList(81);
        }

        internal List<SETCardDTO> DestributeCards(int cardAmount)
        {
            List<SETCardDTO> newCards = new(cardAmount);

            for (int i = 0; i < cardAmount; i++) 
            {
                var cardDTO = SETCardData[cards[cardCursor]];
                newCards.Add(cardDTO);
                cardCursor++;
            }
            
            placedCards.AddRange(newCards);

            return newCards;
        }

        static byte[] GetRandomByteList(int n)
        {
            byte[] bytes = new byte[n];
            for (int i = 0; i < n; i++)
                bytes[i] = (byte)i;
            System.Random r = new System.Random();
            return bytes.OrderBy(x => r.Next()).ToArray();
        }

        internal async Task<SETGamePlayer> StartGuessProcess(string connectionId, IClientProxy clientGroup)
        {
            guessCancelTokenSource?.Cancel();
            guessCancelTokenSource = new CancellationTokenSource();
            var player = players.FirstOrDefault(player => player.ConnectionId == connectionId);
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(guessTime, guessCancelTokenSource.Token);
                    player.AddWrongScore();
                    await clientGroup.SendAsync(SETGameMessageNames.PlayerGuess, new GuessResultResponse
                    {
                        ConnectionId = player.ConnectionId,
                        GuessedCorrect = false,
                        WrongScore = player.WrongScore,
                        CorrectScore = player.CorrectScore,
                        GuessedCards = null,
                    });
                }
                catch (OperationCanceledException) { }
            });
            
            return player;
        }

        internal bool ProcessGuess(string connectionId, List<SETCardDTO> guessCards, out SETGamePlayer player)
        {
            guessCancelTokenSource.Cancel();
            player = players.FirstOrDefault(player => player.ConnectionId == connectionId);
            bool isCorrect = SETGameUtilities.IsSET(guessCards[0], guessCards[1], guessCards[2]);
            if (isCorrect)
            {
                player.AddCorrectScore();

                foreach(var guessCard in guessCards)
                {
                    int index = placedCards.FindIndex((card) => card.Equals(guessCard));
                    placedCards.RemoveAt(index);
                }
            }
            else
                player.AddWrongScore();

            return isCorrect;
                
        }

        internal bool? ProcessCardVote(string connectionId, bool vote)
        {
            var player = players.FirstOrDefault(player => player.ConnectionId == connectionId);
            player.SetVote(vote);

            int yesVote = players.Where(p => p.IsVotePositive == true).Count();
            int noVote = players.Where(p => p.IsVotePositive == false).Count();

            if(yesVote + noVote == players.Count())
            {
                foreach (var p in players)
                    p.SetVote(null);

                return yesVote >= noVote;
            }

            return null;
        }
    }
}
