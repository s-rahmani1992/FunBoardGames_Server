using FunBoardGames.Database;
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FunBoardGames.App.Authentication
{
    public class AuthenticationService
    {
        private readonly FunBoardGamesDbContext _dbContext;

        public AuthenticationService(FunBoardGamesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AuthenticationResponseMessage> SignUp(SignUpRequestMessage request)
        {
            if (string.IsNullOrWhiteSpace(request.PlayerName) || string.IsNullOrWhiteSpace(request.DeviceId))
            {
                return CreateErrorResponse(AuthenticationErrorCode.InvalidRequest);
            }

            var authToken = GenerateAuthToken();

            var userCredentials = new UserCredentials
            {
                Name = request.PlayerName,
                AuthTokenHash = HashToken(authToken),
                DeviceId = request.DeviceId,
                JoinedAt = DateTimeOffset.UtcNow,
                LastLoginAt = DateTimeOffset.UtcNow,
            };

            _dbContext.UserCredentials.Add(userCredentials);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return CreateErrorResponse(AuthenticationErrorCode.UserAlreadyExists);
            }

            return new AuthenticationResponseMessage
            {
                ProfileDTO = new UserProfileDTO
                {
                    UserId = userCredentials.Id,
                    PlayerName = userCredentials.Name,
                    ConnectionId = string.Empty // This can be set later when the user connects
                },
                AuthToken = authToken,
                ErrorCode = AuthenticationErrorCode.None,
            };
        }

        public async Task<AuthenticationResponseMessage> SignIn(SignInRequestMessage request)
        {
            if (string.IsNullOrWhiteSpace(request.PlayerName) ||
                string.IsNullOrWhiteSpace(request.DeviceId) ||
                string.IsNullOrWhiteSpace(request.AuthToken))
            {
                return CreateErrorResponse(AuthenticationErrorCode.InvalidRequest);
            }

            var userCredentials = await _dbContext.UserCredentials.FirstOrDefaultAsync(u =>
                u.Name == request.PlayerName &&
                u.DeviceId == request.DeviceId &&
                u.DeletedAt == null);

            if (userCredentials == null)
            {
                return CreateErrorResponse(AuthenticationErrorCode.UserNotFound);
            }

            if (userCredentials.AuthTokenHash != HashToken(request.AuthToken))
            {
                return CreateErrorResponse(AuthenticationErrorCode.InvalidCredentials);
            }

            userCredentials.LastLoginAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            return new AuthenticationResponseMessage
            {
                ProfileDTO = new UserProfileDTO
                {
                    UserId = userCredentials.Id,
                    PlayerName = userCredentials.Name,
                    ConnectionId = string.Empty // This can be set later when the user connects
                },
                AuthToken = request.AuthToken,
                ErrorCode = AuthenticationErrorCode.None,
            };
        }

        private static AuthenticationResponseMessage CreateErrorResponse(AuthenticationErrorCode errorCode)
        {
            return new AuthenticationResponseMessage
            {
                ProfileDTO = null,
                AuthToken = string.Empty,
                ErrorCode = errorCode,
            };
        }

        private static string GenerateAuthToken(int length = 12)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[bytes[i] % chars.Length];
            }
            return new string(result);
        }

        private static string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes);
        }
    }
}
