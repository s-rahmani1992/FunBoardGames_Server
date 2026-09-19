using FunBoardGames.App.Authentication;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App
{
    public partial class GameHub
    {
        [HubMethodName(AuthenticationMessageNames.SignUp)]
        public async Task SignUpRequest(SignUpRequestMessage signUpMsg)
        {
            var authService = _provider.GetRequiredService<AuthenticationService>();
            var response = await authService.SignUp(signUpMsg);
            await CompleteAuthenticationRequest(AuthenticationMessageNames.SignIn, response);
        }

        [HubMethodName(AuthenticationMessageNames.SignIn)]
        public async Task SignInRequest(SignInRequestMessage signInMsg)
        {
            var authService = _provider.GetRequiredService<AuthenticationService>();
            var response = await authService.SignIn(signInMsg);
            await CompleteAuthenticationRequest(AuthenticationMessageNames.SignIn, response);
        }

        async Task CompleteAuthenticationRequest(string messageName, AuthenticationResponseMessage response)
        {
            if (response.ErrorCode == AuthenticationErrorCode.None && response.ProfileDTO != null)
            {
                Context.Items[ContextNames.Profile] = new Profile(response.ProfileDTO);
            }

            await Clients.Caller.SendAsync(messageName, response);
        }
    }
}
