using FunBoardGames.Network.SignalR.Shared.SET;

namespace FunBoardGames.App.SETGame
{
    public enum TripleComparisonResult : byte
    {
        ALL_SAME = 0,
        ALL_DIFFERRENT = 1,
        NONE = 2,
    }

    public static class SETGameUtilities
    {
        public static TripleComparisonResult CompareItems<T>(T item1, T item2, T item3)
        {
            if (item1.Equals(item2))
            {
                if (item3.Equals(item2)) return TripleComparisonResult.ALL_SAME;
                else return TripleComparisonResult.NONE;
            }
            else
            {
                if (item3.Equals(item1) || item3.Equals(item2)) return TripleComparisonResult.NONE;
                else return TripleComparisonResult.ALL_DIFFERRENT;
            }
        }

        public static bool IsSET(SETCardDTO card1, SETCardDTO card2, SETCardDTO card3)
        {
            if (CompareItems(card1.Color, card2.Color, card3.Color) == TripleComparisonResult.NONE)
                return false;
            
            if (CompareItems(card1.CountIndex, card2.CountIndex, card3.CountIndex) == TripleComparisonResult.NONE)
                return false;
            
            if(CompareItems(card1.Shape, card2.Shape, card3.Shape) == TripleComparisonResult.NONE)
                return false;
            
            if(CompareItems(card1.Shading, card2.Shading, card3.Shading) == TripleComparisonResult.NONE)
                return false;

            return true;
        }
    }
}
