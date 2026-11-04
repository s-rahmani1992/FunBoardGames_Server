
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;

namespace FunBoardGames.App.Core
{
    public abstract class GameController(uint id)
    {
        public abstract int PlayerCount { get; }
        public abstract int ConnectedPlayerCount { get; }
        public abstract int GameId {  get; }
        public abstract BoardGameType GameType { get; }
        public abstract bool IsOpen { get; }
        public bool IsStarted { get; protected set; }

        public abstract bool AddPlayer(Profile profile);
        public abstract bool RemovePlayer(int UserId);
        public virtual Task OnPlayerLeftAsync(int userId) => Task.CompletedTask;
        public virtual void OnRemoved() { }
        protected abstract Task SendPlayerJoinedAsync(IHubCallerClients clients, int newUserId);
        protected abstract Task SendJoinedRoomAsync(IHubCallerClients clients);

        public async Task SendJoinMessagesAsync(IHubCallerClients clients, int newUserId)
        {
            await SendPlayerJoinedAsync(clients, newUserId);
            await SendJoinedRoomAsync(clients);
        }

        public uint RoomId { get; private set; } = id;
        public string GroupKey => $"{GameType}_{GameId}_{RoomId}";
        public SemaphoreSlim RoomLock { get; } = new(1, 1);
    }
}
