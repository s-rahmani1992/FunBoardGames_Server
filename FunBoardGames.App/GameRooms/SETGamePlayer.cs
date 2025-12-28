
namespace FunBoardGames.App.GameRooms
{
    public class SETGamePlayer
    {
        public string Name { get; set; }
        public string ConnectionId { get; set; }
        public bool IsReady { get; set; } = false;
        public bool IsGameLoaded { get; set; }
        public int WrongScore { get; set; } = 0;
        public int CorrectScore { get; set; } = 0;
        public bool? IsVotePositive { get; set; }

        public int AddWrongScore(int score = 1)
        {
            WrongScore += score;
            return WrongScore;
        }

        public int AddCorrectScore(int score = 1)
        {
            CorrectScore += score;
            return CorrectScore;
        }

        public void SetVote(bool? isPositive)
        {
            IsVotePositive = isPositive;
        }

        public SETGamePlayer(string name, string connectionId)
        {
            Name = name;
            ConnectionId = connectionId;
        }

        public void SetReady(bool ready) 
        { 
            IsReady = ready;
        }

        internal void SetLoaded()
        {
            IsGameLoaded = true;
        }
    }
}
