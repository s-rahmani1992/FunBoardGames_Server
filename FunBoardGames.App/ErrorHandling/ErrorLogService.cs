using FunBoardGames.Database;
using FunBoardGames.Domain.ErrorHandling;
using Microsoft.EntityFrameworkCore;

namespace FunBoardGames.App.ErrorHandling
{
    public class ErrorLogService
    {
        private readonly IDbContextFactory<FunBoardGamesDbContext> _dbContextFactory;
        private readonly ILogger<ErrorLogService> _logger;

        public ErrorLogService(IDbContextFactory<FunBoardGamesDbContext> dbContextFactory, ILogger<ErrorLogService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<int> LogAsync(ErrorType type, int? userId, string messageName, string text)
        {
            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                return await dbContext.AddErrorLog(new ErrorLog
                {
                    Type = type,
                    UserId = userId,
                    MessageName = messageName,
                    Text = text,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store {ErrorType} error log of '{MessageName}' for user {UserId}: {Text}",
                    type, messageName, userId, text);
                return 0;
            }
        }

        public Task<int> LogExceptionAsync(int? userId, string messageName, Exception exception)
        {
            return LogAsync(ErrorType.Exception, userId, messageName, exception.ToString());
        }
    }
}
