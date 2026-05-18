namespace DialogueGameEngine.Persistence.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using DialogueGameEngine.Core;

// Each ID type serializes as a plain JSON string rather than { "value": "..." }
public sealed class CharacterIdConverter : JsonConverter<CharacterId>
{
    public override CharacterId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(IdConverterHelpers.ReadString(ref r, "character id"));
    public override void Write(Utf8JsonWriter w, CharacterId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

public sealed class CharacterIdNullableConverter : JsonConverter<CharacterId?>
{
    public override CharacterId? Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) =>
        r.TokenType == JsonTokenType.Null ? null : new CharacterId(IdConverterHelpers.ReadString(ref r, "character id"));
    public override void Write(Utf8JsonWriter w, CharacterId? v, JsonSerializerOptions o)
    {
        if (v is null) w.WriteNullValue();
        else w.WriteStringValue(v.Value.Value);
    }
}

public sealed class SceneIdConverter : JsonConverter<SceneId>
{
    public override SceneId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(IdConverterHelpers.ReadString(ref r, "scene id"));
    public override void Write(Utf8JsonWriter w, SceneId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

public sealed class SceneIdNullableConverter : JsonConverter<SceneId?>
{
    public override SceneId? Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) =>
        r.TokenType == JsonTokenType.Null ? null : new SceneId(IdConverterHelpers.ReadString(ref r, "scene id"));
    public override void Write(Utf8JsonWriter w, SceneId? v, JsonSerializerOptions o)
    {
        if (v is null) w.WriteNullValue();
        else w.WriteStringValue(v.Value.Value);
    }
}

public sealed class AttributeIdConverter : JsonConverter<AttributeId>
{
    public override AttributeId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(IdConverterHelpers.ReadString(ref r, "attribute id"));
    public override void Write(Utf8JsonWriter w, AttributeId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

public sealed class FlagIdConverter : JsonConverter<FlagId>
{
    public override FlagId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(IdConverterHelpers.ReadString(ref r, "flag id"));
    public override void Write(Utf8JsonWriter w, FlagId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

public sealed class ChoiceIdConverter : JsonConverter<ChoiceId>
{
    public override ChoiceId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(IdConverterHelpers.ReadString(ref r, "choice id"));
    public override void Write(Utf8JsonWriter w, ChoiceId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

public sealed class ModifierIdConverter : JsonConverter<ModifierId>
{
    public override ModifierId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(IdConverterHelpers.ReadString(ref r, "modifier id"));
    public override void Write(Utf8JsonWriter w, ModifierId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

internal static class IdConverterHelpers
{
    internal static string ReadString(ref Utf8JsonReader reader, string label)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected {label} to be a JSON string.");

        return reader.GetString()
            ?? throw new JsonException($"Expected {label} to be a non-null JSON string.");
    }
}
