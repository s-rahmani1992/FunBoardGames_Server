
using System.Text.Json.Serialization;

namespace FunBoardGames.Network.SignalR.Shared
{
    public class Profile
    {
        public Profile()
        {
        }

        public Profile(Profile other)
        {
            Id = other.Id;
            UserId = other.UserId;
            PlayerName = other.PlayerName;
            Avatar = other.Avatar;
        }

        [JsonIgnore]
        public int Id { get; set; }

        public int UserId { get; set; }

        public string PlayerName { get; set; } = string.Empty;

        public int Avatar { get; set; }
    }
}
