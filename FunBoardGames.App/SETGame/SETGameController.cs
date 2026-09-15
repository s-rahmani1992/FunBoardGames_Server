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

        public bool IsDeckEmpty => RemainingCardCount <= 0 || placedCards.Count >= game.VisibleCardCount;

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
            ArrangeDeckForSET(cardAmount);
            List<SETCardDTO> newCards = deck.GetRange(cardCursor, cardAmount);
            cardCursor += cardAmount;
            
            placedCards.AddRange(newCards);
            hintCards.Clear();
            hintCards = SETGameUtilities.GetAvailableSET(placedCards).ToList();

            return newCards;
        }

        void ArrangeDeckForSET(int cardAmount)
        {
            if (cardAmount <= 0)
                return;

            int dealEnd = cardCursor + cardAmount;
            if (SETGameUtilities.GetAvailableSET(placedCards.Concat(deck.GetRange(cardCursor, cardAmount))).Any())
                return;

            List<int>? setDeckIndices = FindDealableSET(cardAmount);
            if (setDeckIndices == null)
                return;

            // Swap the SET cards that lie outside the dealt range into it, onto slots that are not part of the SET
            var freeSlots = Enumerable.Range(cardCursor, cardAmount).Where(slot => !setDeckIndices.Contains(slot));
            var outsideIndices = setDeckIndices.Where(index => index >= dealEnd);
            foreach (var (outsideIndex, slot) in outsideIndices.Zip(freeSlots))
                (deck[slot], deck[outsideIndex]) = (deck[outsideIndex], deck[slot]);
        }

        List<int>? FindDealableSET(int cardAmount)
        {
            // Deck index of every candidate card, -1 for cards already on the table
            var candidates = placedCards.Select(card => (card, index: -1))
                .Concat(deck.Skip(cardCursor).Select((card, i) => (card, index: cardCursor + i)))
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            var candidateIndices = candidates.ToDictionary(candidate => GetCardKey(candidate.card), candidate => candidate.index);

            for (int i = 0; i < candidates.Count - 1; i++)
            {
                for (int j = i + 1; j < candidates.Count; j++)
                {
                    var thirdCard = SETGameUtilities.GetThirdSETCard(candidates[i].card, candidates[j].card);
                    if (candidateIndices.TryGetValue(GetCardKey(thirdCard), out int thirdIndex) == false)
                        continue;

                    List<int> deckIndices = new[] { candidates[i].index, candidates[j].index, thirdIndex }
                        .Where(index => index >= 0)
                        .ToList();

                    if (deckIndices.Count <= cardAmount)
                        return deckIndices;
                }
            }

            return null;
        }

        static (byte, byte, byte, byte) GetCardKey(SETCardDTO card) => (card.Color, card.Shape, card.CountIndex, card.Shading);

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
