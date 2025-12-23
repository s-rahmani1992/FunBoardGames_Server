
namespace FunBoardGames.Network.SignalR.Shared
{
    public static class AuthenticationMessageNames
    {
        public const string Login = "Login";
    }

    public class UserProfileDTO
    {
        public string PlayerName { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
    }

    public class LoginRequestMessage
    {
        public string PlayerName { get; set; } = string.Empty;
    }

    public class LoginResponseMessage
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public UserProfileDTO Profile { get; set; }
    }
}
