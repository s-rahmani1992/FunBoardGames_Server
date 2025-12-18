using FunBoardGames.App;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.MapHub<GameHub>("/game");

app.Run();
