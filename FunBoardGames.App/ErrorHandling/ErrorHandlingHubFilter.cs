using FunBoardGames.Domain.ErrorHandling;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App.ErrorHandling
{
    public class ErrorHandlingHubFilter : IHubFilter
    {
        private const string UnexpectedErrorText = "Unexpected error";

        private readonly ErrorLogService _errorLogService;
        private readonly ILogger<ErrorHandlingHubFilter> _logger;

        public ErrorHandlingHubFilter(ErrorLogService errorLogService, ILogger<ErrorHandlingHubFilter> logger)
        {
            _errorLogService = errorLogService;
            _logger = logger;
        }

        public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            try
            {
                return await next(invocationContext);
            }
            catch (InvalidStateException ex)
            {
                await ReportAsync(invocationContext, ErrorType.InvalidState, ex.Message, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                await ReportAsync(invocationContext, ErrorType.Exception, ex.ToString(), UnexpectedErrorText);
                return null;
            }
        }

        public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
        {
            try
            {
                await next(context, exception);
            }
            catch (Exception ex)
            {
                // The connection is already gone, so there is nobody to notify.
                await _errorLogService.LogExceptionAsync(GetUserId(context.Context), nameof(Hub.OnDisconnectedAsync), ex);
            }
        }

        private async Task ReportAsync(HubInvocationContext invocationContext, ErrorType type, string logText, string clientText)
        {
            var messageName = invocationContext.HubMethodName;
            var errorId = await _errorLogService.LogAsync(type, GetUserId(invocationContext.Context), messageName, logText);

            try
            {
                await invocationContext.Hub.Clients.Caller.SendAsync(ErrorMessageNames.Error, new ErrorMessage
                {
                    Type = type == ErrorType.InvalidState ? ErrorMessageType.InvalidState : ErrorMessageType.Exception,
                    MessageName = messageName,
                    Text = clientText,
                    ErrorId = errorId,
                });
            }
            catch (Exception ex)
            {
                // Most likely the caller disconnected while the request was running.
                _logger.LogWarning(ex, "Failed to send error message of '{MessageName}' to the caller", messageName);
            }
        }

        private static int? GetUserId(HubCallerContext context)
        {
            return context.Items.TryGetValue(ContextNames.Profile, out var profile) && profile is Profile p ? p.UserId : null;
        }
    }
}
