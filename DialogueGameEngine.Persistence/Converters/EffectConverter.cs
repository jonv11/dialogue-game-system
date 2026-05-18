namespace DialogueGameEngine.Persistence.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using DialogueGameEngine.Core;

public sealed class EffectConverter : JsonConverter<IEffect>
{
    public override IEffect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("$type", out var typeProp))
            throw new JsonException("Missing '$type' discriminator on effect.");

        var typeName = typeProp.GetString();
        var json = root.GetRawText();

        return typeName switch
        {
            "ChangeAttributeEffect"  => JsonSerializer.Deserialize<ChangeAttributeEffect>(json, options)!,
            "SetAttributeEffect"     => JsonSerializer.Deserialize<SetAttributeEffect>(json, options)!,
            "SetFlagEffect"          => JsonSerializer.Deserialize<SetFlagEffect>(json, options)!,
            "ClearFlagEffect"        => JsonSerializer.Deserialize<ClearFlagEffect>(json, options)!,
            "MoveToSceneEffect"      => JsonSerializer.Deserialize<MoveToSceneEffect>(json, options)!,
            "ConditionalEffect"      => JsonSerializer.Deserialize<ConditionalEffect>(json, options)!,
            _ => throw new JsonException($"Unknown effect type: '{typeName}'")
        };
    }

    public override void Write(Utf8JsonWriter writer, IEffect value, JsonSerializerOptions options)
    {
        var typeName = value.GetType().Name;
        // Serialize the concrete type to a document, then inject $type
        var concreteJson = JsonSerializer.SerializeToDocument(value, value.GetType(), options);
        writer.WriteStartObject();
        writer.WriteString("$type", typeName);
        foreach (var prop in concreteJson.RootElement.EnumerateObject())
            prop.WriteTo(writer);
        writer.WriteEndObject();
    }
}
