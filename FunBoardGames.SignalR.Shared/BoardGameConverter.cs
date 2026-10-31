using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FunBoardGames.Network.SignalR.Shared
{
    public class BoardGameConverter : JsonConverter<BoardGameData>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(BoardGameData);

        public override BoardGameData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var gameType = (BoardGameType)ReadGameType(document.RootElement);
            var json = document.RootElement.GetRawText();

            return gameType switch
            {
                BoardGameType.SET => JsonSerializer.Deserialize<SETGameData>(json, options)!,
                BoardGameType.CantStop => JsonSerializer.Deserialize<CantStopGameData>(json, options)!,
                _ => throw new NotSupportedException($"Unsupported game type: {gameType}"),
            };
        }

        private static int ReadGameType(JsonElement root)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, nameof(BoardGameData.GameType), StringComparison.OrdinalIgnoreCase))
                    return property.Value.GetInt32();
            }

            throw new JsonException($"Missing '{nameof(BoardGameData.GameType)}' discriminator in BoardGameData payload.");
        }

        public override void Write(Utf8JsonWriter writer, BoardGameData value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case SETGameData setGame:
                    JsonSerializer.Serialize(writer, setGame, options);
                    break;
                case CantStopGameData cantStopGame:
                    JsonSerializer.Serialize(writer, cantStopGame, options);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported game DTO type: {value.GetType()}");
            }
        }
    }
}
