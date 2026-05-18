namespace DialogueGameEngine.Persistence;

using System.Text.Json;
using System.Text.Json.Serialization;
using DialogueGameEngine.Persistence.Converters;

public static class SerializerOptionsFactory
{
    /// <summary>
    /// Creates the JsonSerializerOptions for all persistence operations.
    /// Includes polymorphic converters for ICondition and IEffect, and camelCase property names.
    /// </summary>
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new ConditionConverter(),
                new EffectConverter(),
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new CharacterIdConverter(),
                new CharacterIdNullableConverter(),
                new SceneIdConverter(),
                new SceneIdNullableConverter(),
                new AttributeIdConverter(),
                new FlagIdConverter(),
                new ChoiceIdConverter(),
                new ModifierIdConverter(),
            }
        };
        return options;
    }
}
