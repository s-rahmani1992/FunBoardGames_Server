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
        DbSet<BoardGameData> Games => Set<BoardGameData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new UserCredentialConfiguration().Configure(modelBuilder.Entity<UserCredentials>());

            modelBuilder.HasPostgresEnum<GameType>(name: "game_type");

            new BoardGameConfiguration().Configure(modelBuilder.Entity<BoardGameData>());
            
            new SETGameConfiguration().Configure(modelBuilder.Entity<SETGameData>());

            new CantStopGameConfiguration().Configure(modelBuilder.Entity<CantStopGameData>());

        }

        public async Task<UserCredentials> GetUserCredentials(string playerName, string deviceId)
        {
            return await UserCredentials.FirstOrDefaultAsync(uc => uc.Name == playerName && uc.DeviceId == deviceId && uc.DeletedAt == null);
        }

        public async Task<AuthenticationErrorCode> AddNewUser(string playerName, string deviceId, string authTokenHash)
        {
            var userCredentials = new UserCredentials
            {
                Name = playerName,
                AuthTokenHash = authTokenHash,
                DeviceId = deviceId,
                JoinedAt = DateTimeOffset.UtcNow,
                LastLoginAt = DateTimeOffset.UtcNow,
            };
            UserCredentials.Add(userCredentials);

            try
            {
                await SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return AuthenticationErrorCode.UserAlreadyExists;
            }

            return AuthenticationErrorCode.None;
        }

        public async Task<BoardGameData> GetGameById(uint gameId)
        {
            return await Games.FirstOrDefaultAsync(g => g.Id == gameId && g.Deleted_At == null);
        }

        public async Task<BoardGameData> GetGameByType(GameType gameType)
        {
            return await Games.FirstOrDefaultAsync(g => g.GameType == gameType && g.Deleted_At == null);
        }

        public async Task<List<BoardGameData>> GetAllGames()
        {
            return await Games.Where(g => g.Deleted_At == null).ToListAsync();
        }
    }
}
