
using FunBoardGames.App;
using FunBoardGames.App.Authentication;
using FunBoardGames.App.ErrorHandling;
using FunBoardGames.App.Lobby;

using FunBoardGames.Database;

using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR(o =>
{
    o.KeepAliveInterval = TimeSpan.FromSeconds(2);
    o.ClientTimeoutInterval = TimeSpan.FromSeconds(10); // keep >= 2x the
    o.AddFilter<ErrorHandlingHubFilter>();
});

var connectionString = builder.Configuration.GetConnectionString("FunBoardGamesDatabase")
    ?? throw new InvalidOperationException("Connection string 'FunBoardGamesDatabase' not found.");
builder.Services.AddFunBoardGamesDatabase(connectionString);

builder.Services.AddSingleton<ErrorLogService>();
builder.Services.AddSingleton<AuthenticationService>();
builder.Services.AddSingleton<MatchMakingService>();

var app = builder.Build();
app.MapGet("/", () => "App is running");

app.MapHub<GameHub>("/game");

app.Run();
