using FunBoardGames.App.Core;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.SET;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App.SETGame
{
    public class SETGameController : GameControllerT<SETGamePlayer, SETGameData>
    {
        private static List<SETCardDTO> SETCardData;

        byte[] cards;
        int cardCursor = 0;
        List<SETCardDTO> placedCards = [];
        List<SETCardDTO> hintCards = [];

        CancellationTokenSource guessCancelTokenSource;

        //const int guessTime = 7000;

        public bool HasEnoughCards => cardCursor >= SETCardData.Count() - 1 || placedCards.Count >= 12;

        public List<SETPlayerResultDTO> GetFinalResults()
        {
            List<SETPlayerResultDTO> results = [];
            foreach(var player in players)
            {
                results.Add(new SETPlayerResultDTO
                {
                    ConnectionId = player.ConnectionId,
                    Corrects = player.CorrectScore,
                    Wrongs = player.WrongScore,
                });
            }

            results.Sort((a, b) => (b.Corrects - b.Wrongs).CompareTo(a.Corrects - a.Wrongs));
            return results;
        }

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

        public SETGameController(uint id, SETGameData entity) : base(id, entity, (name, connectionId) => new SETGamePlayer(name, connectionId))
        {
            GroupKey = "SET_" + RoomId;
        }

        public override int RequiredPlayerCount => 2;

        public override RoomInfoDTO GetInfo()
        {
            return new RoomInfoDTO
            {
                GameType = BoardGameType.SET,
                Id = RoomId,
                MaxPlayers = RequiredPlayerCount,
                PlayerCount = players.Count(),
                Name = RoomName,
            };
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
            hintCards.Clear();
            hintCards = SETGameUtilities.GetAvailableSET(placedCards).ToList();

            return newCards;
        }

        static byte[] GetRandomByteList(int n)
        {
            byte[] bytes = new byte[n];
            for (int i = 0; i < n; i++)
                bytes[i] = (byte)i;
            Random r = new Random();
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
                    await Task.Delay((int)(7000), guessCancelTokenSource.Token);
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

        internal bool CheckAnySETOnTable()
        {
            hintCards = SETGameUtilities.GetAvailableSET(placedCards).ToList();
            return hintCards.Count > 0;
        }
    }
}
