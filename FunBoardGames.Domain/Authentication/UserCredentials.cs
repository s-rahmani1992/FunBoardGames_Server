
using System;

namespace FunBoardGames.Domain.Authentication
{
    public class UserCredentials
    {
        public int Id { get; set; }
        public string AuthTokenHash { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTimeOffset JoinedAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
