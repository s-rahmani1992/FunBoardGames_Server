using FunBoardGames.App;
using FunBoardGames.App.Authentication;
using FunBoardGames.App.Services;
using FunBoardGames.Database;
using Microsoft.Azure.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("FunBoardGamesDatabase")
    ?? throw new InvalidOperationException("Connection string 'FunBoardGamesDatabase' not found.");
builder.Services.AddFunBoardGamesDatabase(connectionString);

builder.Services.AddSingleton<LobbyService>();
builder.Services.AddScoped<AuthenticationService>();

var app = builder.Build();
app.MapGet("/", () => "App is running");

app.MapHub<GameHub>("/game");

app.Run();
