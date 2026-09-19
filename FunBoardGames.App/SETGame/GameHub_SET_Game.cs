using FunBoardGames.App.Lobby;
using FunBoardGames.App.SETGame;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared.SET;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App
{
    public partial class GameHub
    {
        [HubMethodName(SETGameMessageNames.GameStarted)]
        public async Task SignalGameLoaded()
        {
            SETGameController setController = Context.Items[ContextNames.Room] as SETGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;

            bool allPlayersLoaded = setController.SetPlayerLoaded(profile.UserId);

            if (allPlayersLoaded)
            {
                var gameBeginMessage = setController.PrepareGame();
                await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.GameStarted, gameBeginMessage);
            }
        }

        [HubMethodName(SETGameMessageNames.PlayerGuessStart)]
        public async Task PlayerStartGuess()
        {   
            SETGameController setController = Context.Items[ContextNames.Room] as SETGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;

            var guessStartTime = setController.StartGuessProcess(profile.UserId);
            if (guessStartTime == null)
                return;

            await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.PlayerGuessStart, new PlayerGuessStartMessage
            {
                UserId = profile.UserId,
                GuessStartTime = guessStartTime.Value,
            });
        }

        [HubMethodName(SETGameMessageNames.PlayerGuess)]
        public async Task ProcessGuess(PlayerCardGuessRequest cardGuessMsg)
        {
            SETGameController setController = Context.Items[ContextNames.Room] as SETGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;

            var guessResult = setController.ProcessGuess(profile.UserId, cardGuessMsg.GuessedCards, out var timeoutMessage);
            if (guessResult == null)
                return;

            await Clients.Group(setController.GroupKey).SendAsync(SETGameMessageNames.PlayerGuess, guessResult);

            if (guessResult.FinalScores != null)
            {
                var lobbyService = _provider.GetRequiredService<MatchMakingService>();
                lobbyService.RemoveGame(setController);
            }

            if (timeoutMessage != null)
                await setController.SendRoundTimeout(timeoutMessage);
        }

        [HubMethodName(SETGameMessageNames.CardHint)]
        public async Task UseCardHint()
        {
            SETGameController setController = Context.Items[ContextNames.Room] as SETGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;
            var hintResponse = setController.UseCardHint(profile.UserId);
            if (hintResponse == null)
                return;

            await Clients.Caller.SendAsync(SETGameMessageNames.CardHint, hintResponse);
            await Clients.OthersInGroup(setController.GroupKey).SendAsync(SETGameMessageNames.PlayerUsedHint, new PlayerUsedHintMessage
            {
                UserId = profile.UserId,
                UsedHints = hintResponse.UsedHints,
            });
        }
    }
}
