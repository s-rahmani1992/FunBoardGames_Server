using FunBoardGames.App;
using FunBoardGames.App.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<LobbyService>();
var app = builder.Build();

app.MapHub<GameHub>("/game");

app.Run();
