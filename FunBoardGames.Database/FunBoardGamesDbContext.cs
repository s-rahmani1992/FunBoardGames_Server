using FunBoardGames.Database.Configurations;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.EntityFrameworkCore;

namespace FunBoardGames.Database
{
    public class FunBoardGamesDbContext : DbContext
    {
        public FunBoardGamesDbContext(DbContextOptions<FunBoardGamesDbContext> options)
            : base(options)
        {
        }

        DbSet<UserCredentials> UserCredentials => Set<UserCredentials>();
        DbSet<Profile> Profiles => Set<Profile>();
        DbSet<BoardGameData> Games => Set<BoardGameData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new UserCredentialConfiguration().Configure(modelBuilder.Entity<UserCredentials>());

            new ProfileConfiguration().Configure(modelBuilder.Entity<Profile>());

            modelBuilder.HasPostgresEnum<BoardGameType>(name: "game_type");

            new BoardGameConfiguration().Configure(modelBuilder.Entity<BoardGameData>());
            
            new SETGameConfiguration().Configure(modelBuilder.Entity<SETGameData>());

            new CantStopGameConfiguration().Configure(modelBuilder.Entity<CantStopGameData>());

        }

        public async Task<UserCredentials> GetUserCredentials(int userId, string deviceId)
        {
            return await UserCredentials.FirstOrDefaultAsync(uc => uc.Id == userId && uc.DeviceId == deviceId && uc.DeletedAt == null);
        }

        public async Task<Profile> GetProfile(int userId)
        {
            return await Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<(AuthenticationErrorCode ErrorCode, Profile? Profile)> AddNewUser(string playerName, string deviceId, string authTokenHash)
        {
            // The profile's foreign key needs the user id the database generates, so the two rows are
            // saved in sequence and held together by a transaction wherever the provider supports one.
            await using var transaction = Database.IsRelational() ? await Database.BeginTransactionAsync() : null;

            var userCredentials = new UserCredentials
            {
                AuthTokenHash = authTokenHash,
                DeviceId = deviceId,
                JoinedAt = DateTimeOffset.UtcNow,
                LastLoginAt = DateTimeOffset.UtcNow,
            };
            var userEntry = await UserCredentials.AddAsync(userCredentials);
            Profile profile = new()
            {
                PlayerName = playerName,
                Avatar = 0,
            };
            try
            {
                await SaveChangesAsync();
                profile.UserId = userEntry.Entity.Id;
                await Profiles.AddAsync(profile);
                await SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return (AuthenticationErrorCode.UserAlreadyExists, null);
            }

            if (transaction != null)
                await transaction.CommitAsync();

            return (AuthenticationErrorCode.None, profile);
        }

        public async Task<BoardGameData> GetGameById(uint gameId)
        {
            return await Games.FirstOrDefaultAsync(g => g.Id == gameId && g.Deleted_At == null);
        }

        public async Task<BoardGameData> GetGameByType(BoardGameType gameType)
        {
            return await Games.FirstOrDefaultAsync(g => g.GameType == gameType && g.Deleted_At == null);
        }

        public async Task<List<BoardGameData>> GetAllGames()
        {
            return await Games.Where(g => g.Deleted_At == null).ToListAsync();
        }
    }
}
