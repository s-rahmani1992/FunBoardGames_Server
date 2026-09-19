using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.Database.Entities
{
    public class Profile
    {
        public Profile()
        {
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int Avatar { get; set; }

        public UserProfileDTO ToDTO()
        {
            return new UserProfileDTO
            {
                UserId = UserId,
                PlayerName = PlayerName,
                Avatar = Avatar
            };
        }

        public Profile(Profile other)
        {
            Id = other.Id;
            UserId = other.UserId;
            PlayerName = other.PlayerName;
            Avatar = other.Avatar;
        }

        public Profile(UserProfileDTO dto)
        {
            UserId = dto.UserId;
            PlayerName = dto.PlayerName;
            Avatar = dto.Avatar;
        }
    }
}
