using FunBoardGames.App.Core;
using FunBoardGames.Database.Entities;
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

        readonly IHubContext<GameHub> hubContext;
        readonly Action<GameController> onGameFinished;

        // Guards the game state, which is changed by hub calls and by the round and guess timers concurrently
        readonly object stateLock = new();

        CancellationTokenSource? roundCancelTokenSource;
        CancellationTokenSource? guessCancelTokenSource;
        int? guessingUserId;
        bool isRoundTimeoutPending;
        bool isFinished;
        int roundNumber;

        const int guessTime = 7000;

        IClientProxy ClientGroup => hubContext.Clients.Group(GroupKey);

        public int RemainingCardCount => deck.Count - cardCursor;

        public bool IsDeckEmpty => RemainingCardCount <= 0 || placedCards.Count >= game.VisibleCardCount;

        public List<SETPlayerResultDTO> GetFinalResults()
        {
            List<SETPlayerResultDTO> results = [];
            foreach(var player in players)
            {
                results.Add(new SETPlayerResultDTO
                {
                    UserId = player.Profile.UserId,
                    Corrects = player.CorrectScore,
                    Wrongs = player.WrongScore,
                    IsBusted = player.IsBusted,
                });
            }

            results.Sort((a, b) => b.Corrects.CompareTo(a.Corrects));
            return results;
        }

        public SETGameController(uint id, SETGameData entity, IHubContext<GameHub> hubContext, Action<GameController> onGameFinished)
            : base(id, entity, (profile) => new SETGamePlayer(profile))
        {
            GroupKey = "SET_" + RoomId;
            this.hubContext = hubContext;
            this.onGameFinished = onGameFinished;
        }

        public override void OnRemoved()
        {
            lock (stateLock)
                FinishGame();
        }

        public GameBeginMessage PrepareGame()
        {
            lock (stateLock)
            {
                deck = game.GenerateRandomDeck();
                cardCursor = 0;
                placedCards.Clear();
                hintCards.Clear();
                roundNumber = 0;
                isFinished = false;
                return new GameBeginMessage
                {
                    NewCards = DestributeCards(game.VisibleCardCount),
                    RoundStartTime = StartRoundTimer(),
                };
            }
        }

        internal List<SETCardDTO> DestributeCards(int cardAmount)
        {
            cardAmount = Math.Clamp(cardAmount, 0, RemainingCardCount);
            ArrangeDeckForSET(cardAmount);
            List<SETCardDTO> newCards = deck.GetRange(cardCursor, cardAmount);
            cardCursor += cardAmount;
            
            placedCards.AddRange(newCards);

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

        #region Round Timer

        DateTimeOffset StartRoundTimer()
        {
            var roundStartTime = DateTimeOffset.UtcNow;
            roundNumber++;
            hintCards = SETGameUtilities.GetAvailableSET(placedCards).ToList();
            roundCancelTokenSource?.Cancel();
            roundCancelTokenSource = new CancellationTokenSource();
            isRoundTimeoutPending = false;

            var token = roundCancelTokenSource.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(game.RoundTime), token);
                    await OnRoundTimerEnded(token);
                }
                catch (OperationCanceledException) { }
            });

            return roundStartTime;
        }

        async Task OnRoundTimerEnded(CancellationToken token)
        {
            RoundTimeoutMessage timeoutMessage;
            lock (stateLock)
            {
                if (token.IsCancellationRequested)
                    return;

                // A player is guessing, so the timeout waits until the guess is resolved
                if (guessingUserId != null)
                {
                    isRoundTimeoutPending = true;
                    return;
                }

                timeoutMessage = TimeoutRound();
            }

            await SendRoundTimeout(timeoutMessage);
        }

        RoundTimeoutMessage TimeoutRound()
        {
            var removedCards = SETGameUtilities.GetAvailableSET(placedCards).ToList();
            foreach (var removedCard in removedCards)
                placedCards.Remove(removedCard);

            var timeoutMessage = new RoundTimeoutMessage { RemovedCards = removedCards };

            if (IsDeckEmpty == false)
                timeoutMessage.NewCards = DestributeCards(3);

            if (CheckAnySETOnTable() == false)
                timeoutMessage.FinalScores = FinishGame();
            else
                timeoutMessage.RoundStartTime = StartRoundTimer();

            return timeoutMessage;
        }

        internal async Task SendRoundTimeout(RoundTimeoutMessage timeoutMessage)
        {
            await ClientGroup.SendAsync(SETGameMessageNames.RoundTimeout, timeoutMessage);

            if (timeoutMessage.FinalScores != null)
                onGameFinished(this);
        }

        List<SETPlayerResultDTO> FinishGame()
        {
            isFinished = true;
            isRoundTimeoutPending = false;
            guessingUserId = null;
            roundCancelTokenSource?.Cancel();
            guessCancelTokenSource?.Cancel();
            return GetFinalResults();
        }

        #endregion

        public DateTimeOffset? StartGuessProcess(int userId)
        {
            lock (stateLock)
            {
                if (isFinished || guessingUserId != null)
                    return null;

                var player = players.FirstOrDefault(player => player.Profile.UserId == userId);
                if (player == null || player.IsBusted)
                    return null;

                guessingUserId = userId;
                guessCancelTokenSource = new CancellationTokenSource();

                var token = guessCancelTokenSource.Token;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(guessTime, token);
                        await OnGuessTimerEnded(token);
                    }
                    catch (OperationCanceledException) { }
                });

                return DateTimeOffset.UtcNow;
            }
        }

        async Task OnGuessTimerEnded(CancellationToken token)
        {
            GuessResultResponse? guessResult = null;
            RoundTimeoutMessage? timeoutMessage = null;
            lock (stateLock)
            {
                if (token.IsCancellationRequested)
                    return;

                var player = players.FirstOrDefault(player => player.Profile.UserId == guessingUserId);
                guessingUserId = null;

                if (player != null)
                {
                    AddWrongGuess(player);
                    guessResult = CreateGuessResult(player, false, null);

                    if (IsOnePlayerLeft())
                        guessResult.FinalScores = FinishGame();
                }

                if (guessResult?.FinalScores == null && isRoundTimeoutPending)
                    timeoutMessage = TimeoutRound();
            }

            if (guessResult != null)
            {
                await ClientGroup.SendAsync(SETGameMessageNames.PlayerGuess, guessResult);

                if (guessResult.FinalScores != null)
                    onGameFinished(this);
            }

            if (timeoutMessage != null)
                await SendRoundTimeout(timeoutMessage);
        }

        public GuessResultResponse? ProcessGuess(int userId, List<SETCardDTO> guessCards, out RoundTimeoutMessage? timeoutMessage)
        {
            timeoutMessage = null;
            lock (stateLock)
            {
                if (guessingUserId != userId)
                    return null;

                guessCancelTokenSource?.Cancel();
                guessingUserId = null;
                var player = players.FirstOrDefault(player => player.Profile.UserId == userId);
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
                    AddWrongGuess(player);

                var result = CreateGuessResult(player, isCorrect, guessCards);

                if (isCorrect)
                {
                    if (IsDeckEmpty == false)
                        result.NewCards = DestributeCards(3);

                    // A correct guess starts a new round, which also drops any timeout that was waiting on this guess
                    if (CheckAnySETOnTable() == false)
                        result.FinalScores = FinishGame();
                    else
                        result.RoundStartTime = StartRoundTimer();
                }
                // Only one player can still guess, so the game ends
                else if (IsOnePlayerLeft())
                    result.FinalScores = FinishGame();
                else if (isRoundTimeoutPending)
                    timeoutMessage = TimeoutRound();

                return result;
            }
        }

        void AddWrongGuess(SETGamePlayer player)
        {
            if (player.AddWrongScore() >= game.WrongLimit)
                player.IsBusted = true;
        }

        bool IsOnePlayerLeft() => players.Count(player => player.IsBusted == false) <= 1;

        static GuessResultResponse CreateGuessResult(SETGamePlayer player, bool isCorrect, List<SETCardDTO>? guessCards)
        {
            return new GuessResultResponse
            {
                UserId = player.Profile.UserId,
                GuessedCorrect = isCorrect,
                WrongScore = player.WrongScore,
                CorrectScore = player.CorrectScore,
                IsBusted = player.IsBusted,
                GuessedCards = guessCards,
            };
        }

        private bool CheckAnySETOnTable() => SETGameUtilities.GetAvailableSET(placedCards).Any();

        public CardHintResponse? UseCardHint(int userId)
        {
            lock (stateLock)
            {
                if (isFinished || guessingUserId != null || hintCards.Count == 0)
                    return null;

                var player = players.FirstOrDefault(player => player.Profile.UserId == userId);
                if (player == null || player.IsBusted || player.LastHintRound == roundNumber)
                    return null;

                if (game.HintLimit != null && player.UsedHintCount >= game.HintLimit)
                    return null;

                player.UsedHintCount++;
                player.LastHintRound = roundNumber;

                return new CardHintResponse
                {
                    Card = hintCards[0],
                    UsedHints = player.UsedHintCount,
                };
            }
        }
    }
}
