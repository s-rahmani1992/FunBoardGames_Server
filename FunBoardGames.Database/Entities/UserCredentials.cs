namespace FunBoardGames.Database.Entities
{
    public class UserCredentials
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string DeviceId { get; set; } = string.Empty;

        public DateTimeOffset JoinedAt { get; set; }

        public DateTimeOffset? LastLoginAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }
    }
}
