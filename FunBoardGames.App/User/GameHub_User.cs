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
            if (Context.Items[ContextNames.Profile] is not Profile currentProfile || string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            var dbContextFactory = _provider.GetRequiredService<IDbContextFactory<FunBoardGamesDbContext>>();
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var updatedProfile = await dbContext.UpdatePlayerName(currentProfile.UserId, newName.Trim());
            if (updatedProfile == null)
            {
                return;
            }

            Context.Items[ContextNames.Profile] = new Profile(updatedProfile);
            await Clients.Caller.SendAsync(UserMessageNames.ProfileUpdate, updatedProfile);
        }

        [HubMethodName(UserMessageNames.ChangePlayerAvatar)]
        public async Task ChangePlayerAvatarRequest(int newAvatar)
        {
            if (Context.Items[ContextNames.Profile] is not Profile currentProfile)
            {
                return;
            }
            var dbContextFactory = _provider.GetRequiredService<IDbContextFactory<FunBoardGamesDbContext>>();
            using var dbContext = await dbContextFactory.CreateDbContextAsync();
            var updatedProfile = await dbContext.UpdatePlayerAvatar(currentProfile.UserId, newAvatar);
            if (updatedProfile == null)
            {
                return;
            }
            Context.Items[ContextNames.Profile] = new Profile(updatedProfile);
            await Clients.Caller.SendAsync(UserMessageNames.ProfileUpdate, updatedProfile);
        }
    }
}
