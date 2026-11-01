
using System;

namespace FunBoardGames.Domain.ErrorHandling
{
    public enum ErrorType
    {
        Exception,
        InvalidState,
    }

    public class ErrorLog
    {
        public int Id { get; set; }
        public ErrorType Type { get; set; }
        public int? UserId { get; set; }
        public string MessageName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
