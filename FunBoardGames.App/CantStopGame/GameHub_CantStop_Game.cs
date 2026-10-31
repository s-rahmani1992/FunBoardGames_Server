using FunBoardGames.App.CantStopGame;
using FunBoardGames.App.Lobby;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.Network.SignalR.Shared.CantStop;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App
{
    public partial class GameHub
    {
        [HubMethodName(CantStopGameMessageNames.GameLoaded)]
        public async Task SignalCantStopGameLoaded()
        {
            CantStopGameController cantStopController = Context.Items[ContextNames.Room] as CantStopGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;

            bool allPlayersLoaded = cantStopController.SetPlayerLoaded(profile.UserId);

            if (allPlayersLoaded)
            {
                await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.SendGameData, new GameDataMessage
                {
                    StartPlayerUserId = cantStopController.GetCurrentPlayerId(),
                    StartTime = cantStopController.StartRound(),
                });
            }
        }

        [HubMethodName(CantStopGameMessageNames.RollDice)]
        public async Task RollDice()
        {
            CantStopGameController cantStopController = Context.Items[ContextNames.Room] as CantStopGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;

            if (cantStopController.GetCurrentPlayerId() != profile.UserId)
                return;

            var diceValues = cantStopController.RollDice();
            await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.RollDice, new RollDiceMessage
            {
                diceValues = diceValues,
                IsBusted = cantStopController.IsBusted(),
                StartTime = cantStopController.StartRound(),
            });
        }

        [HubMethodName(CantStopGameMessageNames.PlaceMarkerCone)]
        public async Task PlaceWhiteCone(PlaceWhiteConeRequestMessage placeConeMsg)
        {
            CantStopGameController cantStopController = Context.Items[ContextNames.Room] as CantStopGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;
            if (cantStopController.GetCurrentPlayerId() != profile.UserId)
                return;
            var moveResult = cantStopController.PlaceWhiteCone(placeConeMsg);
            var dices = cantStopController.RollDice();
            if (moveResult.Count() > 0)
            {
                await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.PlaceMarkerCone, new PlaceWhiteConeResponseMessage
                {
                    DiceIndex1 = placeConeMsg.DiceIndex1,
                    DiceIndex2 = placeConeMsg.DiceIndex2,
                    UserId = profile.UserId,
                    UpdatedColumns = moveResult,
                });
            }

            await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.RollDice, new RollDiceMessage
            {
                diceValues = dices,
                IsBusted = cantStopController.IsBusted(),
                StartTime = cantStopController.StartRound(),
            });
        }

        [HubMethodName(CantStopGameMessageNames.PlayRound)]
        public async Task PlayRound(PlaceWhiteConeRequestMessage placeConeMsg)
        {
            CantStopGameController cantStopController = Context.Items[ContextNames.Room] as CantStopGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;

            if (cantStopController.GetCurrentPlayerId() != profile.UserId)
                return;
            var moveResult = cantStopController.PlaceWhiteCone(placeConeMsg);
            var nextId = cantStopController.UpdatePlayerCone();
            var player = cantStopController.GetPlayerByConnectionId(profile.UserId);
            await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.PlayRound, new PlayRoundResponseMessage
            {
                DiceIndex1 = placeConeMsg.DiceIndex1,
                DiceIndex2 = placeConeMsg.DiceIndex2,
                UserId = profile.UserId,
                UpdatedColumns = moveResult,
                NextPlayerUserId = nextId,
                FinalScore = player.Score,
                FinishedColumns = cantStopController.FinishedColumns,
                PlayerCones = player.ConePositions,
                StartTime = cantStopController.StartRound(),
            });

            if (cantStopController.IsFinished())
            {
                await Task.Delay(1000);
                await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.GameFinished, new GameFinishedResponseMessage
                {
                    PlayerScores = cantStopController.GetPlayerScores(),
                });
                var lobbyService = _provider.GetRequiredService<MatchMakingService>();
                lobbyService.RemoveGame(cantStopController);
            }
        }

        [HubMethodName(CantStopGameMessageNames.EndRound)]
        public async Task EndRound()
        {
            CantStopGameController cantStopController = Context.Items[ContextNames.Room] as CantStopGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;

            if (cantStopController.GetCurrentPlayerId() != profile.UserId)
                return;

            var endResult = cantStopController.EndRound();
            endResult.UserId = profile.UserId;
            await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.EndRound, endResult);
        }

        [HubMethodName(CantStopGameMessageNames.ResetRound)]
        public async Task ResetRound()
        {
            CantStopGameController cantStopController = Context.Items[ContextNames.Room] as CantStopGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;
            if (cantStopController.GetCurrentPlayerId() != profile.UserId)
                return;
            bool success = cantStopController.ResetRound();

            if (success)
                await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.ResetRound);
        }

        [HubMethodName(CantStopGameMessageNames.BustSaver)]
        public async Task BustSave()
        {
            CantStopGameController cantStopController = Context.Items[ContextNames.Room] as CantStopGameController;
            var profile = Context.Items[ContextNames.Profile] as Profile;
            if (cantStopController.GetCurrentPlayerId() != profile.UserId)
                return;

            var success = cantStopController.SaveBust();

            if (success)
                await Clients.Group(cantStopController.GroupKey).SendAsync(CantStopGameMessageNames.BustSaver);
        }
    }
}
