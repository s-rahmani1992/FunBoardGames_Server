
namespace FunBoardGames.Network.SignalR.Shared
{
    public static class AuthenticationMessageNames
    {
        public const string SignUp = "SignUp";
        public const string SignIn = "SignIn";
    }

    public class UserProfileDTO
    {
        public int UserId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
    }

    public enum AuthenticationErrorCode
    {
        None = 0,
        UserNotFound = 1,
        InvalidCredentials = 2,
        UserAlreadyExists = 3,
        InvalidRequest = 4,
    }

    public class SignUpRequestMessage
    {
        public string PlayerName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
    }

    public class SignInRequestMessage
    {
        public string PlayerName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
    }

    public class AuthenticationResponseMessage
    {
        public UserProfileDTO? ProfileDTO { get; set; }
        public string AuthToken { get; set; } = string.Empty;
        public AuthenticationErrorCode ErrorCode { get; set; } = AuthenticationErrorCode.None;
    }
}
