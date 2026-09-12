using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunBoardGames.Database.Entities
{
    public enum GameType
    {
        SET,
        CantStop,
    }

    public class BoardGameEntity
    {
        public uint Id { get; set; }
        public GameType GameType { get; set; }
        public string Name { get; set; } = string.Empty;
        public uint PlayerCount { get; set; } = 2;
        public DateTimeOffset Created_At { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? Deleted_At { get; set; } = null;
    }

    public class SETGameEntity : BoardGameEntity
    {
        public float GuessTime { get; set; } = 7.0f;
    }

    public class CantStopGameEntity : BoardGameEntity
    {
        public Dictionary<int, int> BoardData {  get; set; }
    }
}
