
namespace FunBoardGames.Network.SignalR.Shared
{
    public static class ErrorMessageNames
    {
        public const string Error = "Error";
    }

    public enum ErrorMessageType
    {
        Exception = 0,
        InvalidState = 1,
    }

    public class ErrorMessage
    {
        public int ErrorId { get; set; }

        public ErrorMessageType Type { get; set; }

        public string MessageName { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

    }
}
