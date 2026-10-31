namespace FunBoardGames.Network.SignalR.Shared
{
    public class SETGameData : BoardGameData
    {
        public const byte AttributeValueCount = 3;

        public const float GuessTime = 7;

        public int VisibleCardCount { get; set; } = 12;
        public int RoundTime { get; set; } = 30;
        public int WrongLimit { get; set; } = 3;
        public int? HintLimit { get; set; } = null;
        public int? ColorAttribute { get; set; } = null;
        public int? ShapeAttribute { get; set; } = null;
        public int? CountAttribute { get; set; } = null;
        public int? ShadingAttribute { get; set; } = null;
    }
}
