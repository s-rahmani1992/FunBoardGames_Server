using FunBoardGames.SignalR.Shared;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FunBoardGames.Network.SignalR.Shared
{
    /// <summary>
    /// Dispatches GameDTO (de)serialization to the concrete SETGameDTO/CantStopGameDTO type
    /// based on the already-present GameType field, using explicit generic calls so AOT
    /// compilers (e.g. IL2CPP) can see the concrete instantiations statically.
    /// </summary>
    public class GameDTOConverter : JsonConverter<GameDTO>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(GameDTO);

        public override GameDTO Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var gameType = (BoardGameType)ReadGameType(document.RootElement);
            var json = document.RootElement.GetRawText();

            return gameType switch
            {
                BoardGameType.SET => JsonSerializer.Deserialize<SETGameDTO>(json, options)!,
                BoardGameType.CantStop => JsonSerializer.Deserialize<CantStopGameDTO>(json, options)!,
                _ => throw new NotSupportedException($"Unsupported game type: {gameType}"),
            };
        }

        // JsonElement.GetProperty is always case-sensitive, but the hub protocol names
        // properties with its own policy (SignalR's default is camelCase), so the
        // discriminator has to be matched independently of casing.
        private static int ReadGameType(JsonElement root)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, nameof(GameDTO.GameType), StringComparison.OrdinalIgnoreCase))
                    return property.Value.GetInt32();
            }

            throw new JsonException($"Missing '{nameof(GameDTO.GameType)}' discriminator in GameDTO payload.");
        }

        public override void Write(Utf8JsonWriter writer, GameDTO value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case SETGameDTO setGame:
                    JsonSerializer.Serialize(writer, setGame, options);
                    break;
                case CantStopGameDTO cantStopGame:
                    JsonSerializer.Serialize(writer, cantStopGame, options);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported game DTO type: {value.GetType()}");
            }
        }
    }
}
