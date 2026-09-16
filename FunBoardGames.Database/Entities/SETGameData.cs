
using FunBoardGames.Network.SignalR.Shared.SET;

namespace FunBoardGames.Database.Entities
{
    public class SETGameData : BoardGameData
    {
        const byte AttributeValueCount = 3;

        public int VisibleCardCount { get; set; } = 12;
        public int RoundTime { get; set; } = 30;
        public int? ColorAttribute { get; set; } = null;
        public int? ShapeAttribute { get; set; } = null;
        public int? CountAttribute { get; set; } = null;
        public int? ShadingAttribute { get; set; } = null;

        public int AttributeCount => 
            (ColorAttribute == null ? 1 : 0) 
            + (ShapeAttribute == null ? 1 : 0) 
            + (CountAttribute == null ? 1 : 0) 
            + (ShadingAttribute == null ? 1 : 0);

        /// <summary>
        /// Builds every card combination and shuffles them. An attribute with a value (0 to 2) is constant
        /// on every card, while a null attribute varies across all of its values.
        /// </summary>
        public List<SETCardDTO> GenerateRandomDeck()
        {
            var deck =
                from color in GetAttributeValues(ColorAttribute, nameof(ColorAttribute))
                from shape in GetAttributeValues(ShapeAttribute, nameof(ShapeAttribute))
                from count in GetAttributeValues(CountAttribute, nameof(CountAttribute))
                from shading in GetAttributeValues(ShadingAttribute, nameof(ShadingAttribute))
                select new SETCardDTO
                {
                    Color = color,
                    Shape = shape,
                    CountIndex = count,
                    Shading = shading,
                };

            return deck.OrderBy(_ => Random.Shared.Next()).ToList();
        }

        static byte[] GetAttributeValues(int? attribute, string attributeName)
        {
            if (attribute == null)
                return [0, 1, 2];

            if (attribute < 0 || attribute >= AttributeValueCount)
                throw new ArgumentOutOfRangeException(attributeName, attribute, $"Must be null or between 0 and {AttributeValueCount - 1}.");

            return [(byte)attribute.Value];
        }
    }
}
