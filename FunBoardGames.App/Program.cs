using FunBoardGames.App;
using FunBoardGames.App.Services;
using Microsoft.Azure.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR().AddAzureSignalR(builder.Configuration["Azure:SignalR:ConnectionString"]!);
builder.Services.AddSingleton<LobbyService>();
var app = builder.Build();
app.MapGet("/", () => "App is running");

app.MapHub<GameHub>("/game");

app.Run();
