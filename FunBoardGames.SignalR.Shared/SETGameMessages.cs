
using System.Collections.Generic;

namespace FunBoardGames.Network.SignalR.Shared.SET
{
    public static class SETGameMessageNames
    {
        public const string GameLoaded = "SET_GameLoaded";
        public const string DistributeCards = "SET_DistributeCards";
    }

    public class SETCardDTO
    {
        public byte Color { get; set; }
        public byte Shape { get; set; }
        public byte CountIndex { get; set; }
        public byte Shading { get; set; }
    }

    public class  DistributeNewCardsMessage
    {
        public List<SETCardDTO> NewCards { get; set; }
    }
}
