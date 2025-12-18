using Microsoft.AspNetCore.SignalR;
using FunBoardGames.App.Messages;

namespace FunBoardGames.App
{
    public static class MessageNames
    {
        public const string Login = "Login";
    }

    public class GameHub : Hub
    {
        static HashSet<string> connectedUsers = [];

        [HubMethodName(MessageNames.Login)]
        public async Task LoginRequest(LoginRequestMsg loginMsg)
        {
            if(connectedUsers.Add(loginMsg.PlayerName))
            {
                await Clients.Caller.SendAsync(MessageNames.Login, new LoginResponseMsg { Success = true, PlayerName = loginMsg.PlayerName });
                Context.Items["name"] = loginMsg.PlayerName;
            }
            else
            {
                await Clients.Caller.SendAsync(MessageNames.Login, new LoginResponseMsg { Success = false, ErrorMessage = "Username already taken." });
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if(Context.Items.TryGetValue("name", out var nameObj) && nameObj is string name)
            {
                connectedUsers.Remove(name);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
