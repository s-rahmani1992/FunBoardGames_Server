using FunBoardGames.App.Core;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.SET;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App.SETGame
{
    public class SETGameController : GameControllerT<SETGamePlayer, SETGameData>
    {
        List<SETCardDTO> deck = [];
        int cardCursor = 0;
        List<SETCardDTO> placedCards = [];
        List<SETCardDTO> hintCards = [];

        CancellationTokenSource guessCancelTokenSource;

        //const int guessTime = 7000;

        public int RemainingCardCount => deck.Count - cardCursor;

        public bool IsDeckEmpty => RemainingCardCount <= 0 || placedCards.Count >= 12;

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

        internal List<SETCardDTO> PrepareGame()
        {
            deck = game.GenerateRandomDeck();
            cardCursor = 0;
            placedCards.Clear();
            hintCards.Clear();
            return DestributeCards(game.VisibleCardCount);
        }

        internal List<SETCardDTO> DestributeCards(int cardAmount)
        {
            cardAmount = Math.Clamp(cardAmount, 0, RemainingCardCount);
            List<SETCardDTO> newCards = deck.GetRange(cardCursor, cardAmount);
            cardCursor += cardAmount;
            
            placedCards.AddRange(newCards);
            hintCards.Clear();
            hintCards = SETGameUtilities.GetAvailableSET(placedCards).ToList();

            return newCards;
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
