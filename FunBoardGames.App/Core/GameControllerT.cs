
using FunBoardGames.Database.Entities;
using FunBoardGames.Network.SignalR.Shared;

namespace FunBoardGames.App.Core
{
    public abstract class GameControllerT<TPlayer, TGame> : GameController where TPlayer : BoardGamePlayer where TGame : BoardGameData
    {
        protected readonly List<TPlayer> players = [];
        protected readonly TGame game;
        readonly Func<Profile, TPlayer> createPlayer;

        protected GameControllerT(uint id, TGame game, Func<Profile, TPlayer> createPlayer) : base(id)
        {
            this.game = game;
            this.createPlayer = createPlayer;
        }

        public override uint GameId => game.Id;
        public override int PlayerCount => players.Count;
        public override bool IsOpen => players.Count < (int)game.PlayerCount;

        public override bool AddPlayer(Profile profile)
        {
            if(players.Exists(p => p.Profile.UserId == profile.UserId))
                return false;

            players.Add(createPlayer(profile));
            return true;
        }

        public override bool RemovePlayer(int userId)
        {
            var player = players.FirstOrDefault(player => player.Profile.UserId == userId);
            return players.Remove(player);
        }

        public override IEnumerable<PlayerInfoDTO> GetPlayers()
        {
            return players.Select(player => new PlayerInfoDTO
            {
                UserProfile = player.Profile.ToDTO(),
            });
        }

        public override bool SetPlayerLoaded(int userId)
        {
            var player = players.FirstOrDefault(p => p.Profile.UserId == userId);
            player.SetGameLoaded();

            return players.All(player => player.IsGameLoaded);
        }
    }
}
