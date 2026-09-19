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
        public AuthenticationService(IDbContextFactory<FunBoardGamesDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        readonly IDbContextFactory<FunBoardGamesDbContext> _dbContextFactory;

        public async Task<AuthenticationResponseMessage> SignUp(SignUpRequestMessage request)
        {
            if (string.IsNullOrWhiteSpace(request.PlayerName) || string.IsNullOrWhiteSpace(request.DeviceId))
            {
                return CreateErrorResponse(AuthenticationErrorCode.InvalidRequest);
            }

            var authToken = GenerateAuthToken();

            using var _dbContext = await _dbContextFactory.CreateDbContextAsync();

            var (errorCode, profile) = await _dbContext.AddNewUser(request.PlayerName, request.DeviceId, HashToken(authToken));
            if (errorCode != AuthenticationErrorCode.None || profile == null)
            {
                return CreateErrorResponse(errorCode);
            }

            return new AuthenticationResponseMessage
            {
                ProfileDTO = ToProfileDTO(profile),
                AuthToken = authToken,
                ErrorCode = AuthenticationErrorCode.None,
            };
        }

        public async Task<AuthenticationResponseMessage> SignIn(SignInRequestMessage request)
        {
            if (request.UserId <= 0 ||
                string.IsNullOrWhiteSpace(request.DeviceId) ||
                string.IsNullOrWhiteSpace(request.AuthToken))
            {
                return CreateErrorResponse(AuthenticationErrorCode.InvalidRequest);
            }

            using var _dbContext = await _dbContextFactory.CreateDbContextAsync();

            var userCredentials = await _dbContext.GetUserCredentials(request.UserId, request.DeviceId);

            if (userCredentials == null)
            {
                return CreateErrorResponse(AuthenticationErrorCode.UserNotFound);
            }

            if (userCredentials.AuthTokenHash != HashToken(request.AuthToken))
            {
                return CreateErrorResponse(AuthenticationErrorCode.InvalidCredentials);
            }

            var profile = await _dbContext.GetProfile(userCredentials.Id);

            if (profile == null)
            {
                return CreateErrorResponse(AuthenticationErrorCode.UserNotFound);
            }

            userCredentials.LastLoginAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            return new AuthenticationResponseMessage
            {
                ProfileDTO = ToProfileDTO(profile),
                AuthToken = request.AuthToken,
                ErrorCode = AuthenticationErrorCode.None,
            };
        }

        private static UserProfileDTO ToProfileDTO(Profile profile)
        {
            return new UserProfileDTO
            {
                UserId = profile.UserId,
                PlayerName = profile.PlayerName,
                Avatar = profile.Avatar,
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
