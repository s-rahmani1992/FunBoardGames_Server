using FunBoardGames.App.Authentication;
using FunBoardGames.Database;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace FunBoardGames.App.Tests.Authentication
{
    public class AuthenticationServiceTests
    {
        private static FunBoardGamesDbContext CreateDbContext()
        {
            // A fresh, uniquely-named in-memory database per test keeps tests isolated
            // from each other without needing a real PostgreSQL instance.
            var options = new DbContextOptionsBuilder<FunBoardGamesDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FunBoardGamesDbContext(options);
        }

        // Mirrors AuthenticationService's private token-hashing algorithm so tests can
        // seed a row with a known token and verify what SignUp/SignIn store or accept,
        // without needing to expose that implementation detail from the service itself.
        private static string Hash(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes);
        }

        private static async Task<UserCredentials> SeedUserAsync(
            FunBoardGamesDbContext dbContext,
            string name,
            string deviceId,
            string authToken,
            DateTimeOffset? deletedAt = null)
        {
            var user = new UserCredentials
            {
                Name = name,
                DeviceId = deviceId,
                AuthTokenHash = Hash(authToken),
                JoinedAt = DateTimeOffset.UtcNow.AddDays(-1),
                DeletedAt = deletedAt,
            };

            dbContext.UserCredentials.Add(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        #region SignUp

        [Fact]
        public async Task SignUp_WithValidRequest_AddsUserCredentialsRowToDatabase()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            var request = new SignUpRequestMessage
            {
                PlayerName = "Alice",
                DeviceId = "device-123",
            };

            var response = await sut.SignUp(request);

            var savedUser = Assert.Single(dbContext.UserCredentials);
            Assert.Equal("Alice", savedUser.Name);
            Assert.Equal("device-123", savedUser.DeviceId);
            Assert.False(string.IsNullOrEmpty(savedUser.AuthTokenHash));
            Assert.NotEqual(0, savedUser.Id);
            Assert.Equal(AuthenticationErrorCode.None, response.ErrorCode);
        }

        [Fact]
        public async Task SignUp_WithValidRequest_SetsJoinedAtToCurrentUtcTime()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            var before = DateTimeOffset.UtcNow;

            await sut.SignUp(new SignUpRequestMessage { PlayerName = "Bob", DeviceId = "device-456" });

            var after = DateTimeOffset.UtcNow;
            var savedUser = Assert.Single(dbContext.UserCredentials);
            Assert.InRange(savedUser.JoinedAt, before, after);
        }

        [Fact]
        public async Task SignUp_WithValidRequest_ReturnsResponseWithSavedEntityIdAsProfileUserId()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);

            var response = await sut.SignUp(new SignUpRequestMessage { PlayerName = "Carol", DeviceId = "device-789" });

            var savedUser = Assert.Single(dbContext.UserCredentials);
            Assert.NotNull(response.ProfileDTO);
            Assert.Equal(savedUser.Id, response.ProfileDTO!.UserId);
            Assert.Equal("Carol", response.ProfileDTO.PlayerName);
            Assert.Equal(string.Empty, response.ProfileDTO.ConnectionId);
        }

        [Fact]
        public async Task SignUp_WithValidRequest_ReturnsPlaintextAuthTokenButStoresOnlyItsHash()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);

            var response = await sut.SignUp(new SignUpRequestMessage { PlayerName = "Dave", DeviceId = "device-321" });

            var savedUser = Assert.Single(dbContext.UserCredentials);
            Assert.False(string.IsNullOrWhiteSpace(response.AuthToken));
            Assert.NotEqual(response.AuthToken, savedUser.AuthTokenHash); // never stored in plaintext
            Assert.Equal(Hash(response.AuthToken), savedUser.AuthTokenHash);
        }

        [Fact]
        public async Task SignUp_CalledTwiceWithDifferentUsers_CreatesTwoDistinctUsersWithDifferentIdsAndTokens()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);

            var first = await sut.SignUp(new SignUpRequestMessage { PlayerName = "Erin", DeviceId = "device-001" });
            var second = await sut.SignUp(new SignUpRequestMessage { PlayerName = "Frank", DeviceId = "device-002" });

            Assert.Equal(2, dbContext.UserCredentials.Count());
            Assert.NotEqual(first.ProfileDTO!.UserId, second.ProfileDTO!.UserId);
            Assert.NotEqual(first.AuthToken, second.AuthToken);
        }

        [Fact]
        public async Task SignUp_WhenPlayerAlreadyRegisteredOnSameDevice_ReturnsUserAlreadyExistsErrorAndDoesNotAddRow()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            var request = new SignUpRequestMessage { PlayerName = "Grace", DeviceId = "device-555" };
            await sut.SignUp(request);

            var response = await sut.SignUp(request);

            Assert.Equal(AuthenticationErrorCode.UserAlreadyExists, response.ErrorCode);
            Assert.Null(response.ProfileDTO);
            Assert.Equal(string.Empty, response.AuthToken);
            Assert.Single(dbContext.UserCredentials); // still just the first row
        }

        [Fact]
        public async Task SignUp_AfterExistingRegistrationIsSoftDeleted_AllowsReSignUp()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            var deletedUser = await SeedUserAsync(dbContext, "Henry", "device-777", "old-token", deletedAt: DateTimeOffset.UtcNow.AddDays(-1));

            var response = await sut.SignUp(new SignUpRequestMessage { PlayerName = "Henry", DeviceId = "device-777" });

            Assert.Equal(AuthenticationErrorCode.None, response.ErrorCode);
            Assert.NotNull(response.ProfileDTO);
            Assert.Equal(2, dbContext.UserCredentials.Count());
            Assert.NotEqual(deletedUser.Id, response.ProfileDTO!.UserId);
        }

        [Theory]
        [InlineData("", "device-1")]
        [InlineData(" ", "device-1")]
        [InlineData(null, "device-1")]
        [InlineData("Ivy", "")]
        [InlineData("Ivy", " ")]
        [InlineData("Ivy", null)]
        public async Task SignUp_WithMissingPlayerNameOrDeviceId_ReturnsInvalidRequestErrorAndDoesNotAddRow(string? playerName, string? deviceId)
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);

            var response = await sut.SignUp(new SignUpRequestMessage
            {
                PlayerName = playerName!,
                DeviceId = deviceId!,
            });

            Assert.Equal(AuthenticationErrorCode.InvalidRequest, response.ErrorCode);
            Assert.Null(response.ProfileDTO);
            Assert.Empty(dbContext.UserCredentials);
        }

        #endregion

        #region SignIn

        [Fact]
        public async Task SignIn_WithMatchingCredentials_ReturnsSuccessResponseWithProfile()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            var seededUser = await SeedUserAsync(dbContext, "Grace", "device-999", "correct-token");

            var response = await sut.SignIn(new SignInRequestMessage
            {
                PlayerName = "Grace",
                DeviceId = "device-999",
                AuthToken = "correct-token",
            });

            Assert.Equal(AuthenticationErrorCode.None, response.ErrorCode);
            Assert.NotNull(response.ProfileDTO);
            Assert.Equal(seededUser.Id, response.ProfileDTO!.UserId);
            Assert.Equal("Grace", response.ProfileDTO.PlayerName);
            Assert.Equal("correct-token", response.AuthToken);
        }

        [Fact]
        public async Task SignIn_WithMatchingCredentials_UpdatesLastLoginAt()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            var seededUser = await SeedUserAsync(dbContext, "Grace", "device-999", "correct-token");
            var before = DateTimeOffset.UtcNow;

            await sut.SignIn(new SignInRequestMessage { PlayerName = "Grace", DeviceId = "device-999", AuthToken = "correct-token" });

            var after = DateTimeOffset.UtcNow;
            Assert.True(seededUser.LastLoginAt.HasValue);
            Assert.InRange(seededUser.LastLoginAt!.Value, before, after);
        }

        [Fact]
        public async Task SignIn_WhenNoUserMatchesPlayerNameAndDeviceId_ReturnsUserNotFoundError()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);

            var response = await sut.SignIn(new SignInRequestMessage
            {
                PlayerName = "Unknown",
                DeviceId = "device-000",
                AuthToken = "whatever",
            });

            Assert.Equal(AuthenticationErrorCode.UserNotFound, response.ErrorCode);
            Assert.Null(response.ProfileDTO);
        }

        [Fact]
        public async Task SignIn_WhenDeviceIdDoesNotMatchRegisteredDevice_ReturnsUserNotFoundError()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            await SeedUserAsync(dbContext, "Grace", "device-999", "correct-token");

            var response = await sut.SignIn(new SignInRequestMessage
            {
                PlayerName = "Grace",
                DeviceId = "a-different-device",
                AuthToken = "correct-token",
            });

            Assert.Equal(AuthenticationErrorCode.UserNotFound, response.ErrorCode);
            Assert.Null(response.ProfileDTO);
        }

        [Fact]
        public async Task SignIn_WhenAuthTokenDoesNotMatch_ReturnsInvalidCredentialsErrorAndDoesNotUpdateLastLoginAt()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            var seededUser = await SeedUserAsync(dbContext, "Grace", "device-999", "correct-token");

            var response = await sut.SignIn(new SignInRequestMessage
            {
                PlayerName = "Grace",
                DeviceId = "device-999",
                AuthToken = "wrong-token",
            });

            Assert.Equal(AuthenticationErrorCode.InvalidCredentials, response.ErrorCode);
            Assert.Null(response.ProfileDTO);
            Assert.Null(seededUser.LastLoginAt);
        }

        [Fact]
        public async Task SignIn_ForSoftDeletedUser_ReturnsUserNotFoundError()
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);
            await SeedUserAsync(dbContext, "Grace", "device-999", "correct-token", deletedAt: DateTimeOffset.UtcNow.AddHours(-1));

            var response = await sut.SignIn(new SignInRequestMessage
            {
                PlayerName = "Grace",
                DeviceId = "device-999",
                AuthToken = "correct-token",
            });

            Assert.Equal(AuthenticationErrorCode.UserNotFound, response.ErrorCode);
            Assert.Null(response.ProfileDTO);
        }

        [Theory]
        [InlineData("", "device-1", "token")]
        [InlineData(" ", "device-1", "token")]
        [InlineData(null, "device-1", "token")]
        [InlineData("Ivy", "", "token")]
        [InlineData("Ivy", " ", "token")]
        [InlineData("Ivy", null, "token")]
        [InlineData("Ivy", "device-1", "")]
        [InlineData("Ivy", "device-1", " ")]
        [InlineData("Ivy", "device-1", null)]
        public async Task SignIn_WithMissingCredentialField_ReturnsInvalidRequestError(string? playerName, string? deviceId, string? authToken)
        {
            using var dbContext = CreateDbContext();
            var sut = new AuthenticationService(dbContext);

            var response = await sut.SignIn(new SignInRequestMessage
            {
                PlayerName = playerName!,
                DeviceId = deviceId!,
                AuthToken = authToken!,
            });

            Assert.Equal(AuthenticationErrorCode.InvalidRequest, response.ErrorCode);
            Assert.Null(response.ProfileDTO);
        }

        #endregion
    }
}
