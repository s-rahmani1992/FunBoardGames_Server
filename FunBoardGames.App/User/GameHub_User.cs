using FunBoardGames.App.ErrorHandling;
using FunBoardGames.Database;
using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FunBoardGames.App
{
    public partial class GameHub
    {
        [HubMethodName(UserMessageNames.ChangePlayerName)]
        public async Task ChangePlayerNameRequest(string newName)
        {
            if (Context.Items[ContextNames.Profile] is not Profile currentProfile)
            {
                throw new InvalidStateException("User is not signed in.");
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new InvalidStateException("Player name cannot be empty.");
            }

            var dbContextFactory = _provider.GetRequiredService<IDbContextFactory<FunBoardGamesDbContext>>();
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var updatedProfile = await dbContext.UpdatePlayerName(currentProfile.UserId, newName.Trim()) ?? throw new InvalidStateException("Profile was not found.");
            Context.Items[ContextNames.Profile] = new Profile(updatedProfile);
            await Clients.Caller.SendAsync(UserMessageNames.ProfileUpdate, updatedProfile);
        }

        [HubMethodName(UserMessageNames.ChangePlayerAvatar)]
        public async Task ChangePlayerAvatarRequest(int newAvatar)
        {
            if (Context.Items[ContextNames.Profile] is not Profile currentProfile)
            {
                throw new InvalidStateException("User is not signed in.");
            }
            var dbContextFactory = _provider.GetRequiredService<IDbContextFactory<FunBoardGamesDbContext>>();
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var updatedProfile = await dbContext.UpdatePlayerAvatar(currentProfile.UserId, newAvatar) ?? throw new InvalidStateException("Profile was not found.");
            Context.Items[ContextNames.Profile] = new Profile(updatedProfile);
            await Clients.Caller.SendAsync(UserMessageNames.ProfileUpdate, updatedProfile);
        }
    }
}
