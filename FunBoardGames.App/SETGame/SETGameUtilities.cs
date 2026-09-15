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

        /// <summary>
        /// Returns the only card that completes a SET with the two given cards.
        /// </summary>
        public static SETCardDTO GetThirdSETCard(SETCardDTO card1, SETCardDTO card2)
        {
            return new SETCardDTO
            {
                Color = GetThirdValue(card1.Color, card2.Color),
                Shape = GetThirdValue(card1.Shape, card2.Shape),
                CountIndex = GetThirdValue(card1.CountIndex, card2.CountIndex),
                Shading = GetThirdValue(card1.Shading, card2.Shading),
            };
        }

        static byte GetThirdValue(byte value1, byte value2) => value1 == value2 ? value1 : (byte)(3 - value1 - value2);

        public static IEnumerable<SETCardDTO> GetAvailableSET(IEnumerable<SETCardDTO> cards)
        {
            var cardList = cards.ToList();
            int count = cardList.Count;
            for (int i = 0; i < count - 2; i++)
            {
                for (int j = i + 1; j < count - 1; j++)
                {
                    for (int k = j + 1; k < count; k++)
                    {
                        if (IsSET(cardList[i], cardList[j], cardList[k]))
                        {
                            yield return cardList[i];
                            yield return cardList[j];
                            yield return cardList[k];
                            yield break;
                        }
                    }
                }
            }
        }
    }
}
