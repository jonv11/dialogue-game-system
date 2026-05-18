namespace DialogueGameEngine.Persistence.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using DialogueGameEngine.Core;

public sealed class ConditionConverter : JsonConverter<ICondition>
{
    public override ICondition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("$type", out var typeProp))
            throw new JsonException("Missing '$type' discriminator on condition.");

        var typeName = typeProp.GetString();
        var json = root.GetRawText();

        return typeName switch
        {
            "AttributeCondition" => JsonSerializer.Deserialize<AttributeCondition>(json, options)!,
            "FlagCondition"      => JsonSerializer.Deserialize<FlagCondition>(json, options)!,
            "AllCondition"       => JsonSerializer.Deserialize<AllCondition>(json, options)!,
            "AnyCondition"       => JsonSerializer.Deserialize<AnyCondition>(json, options)!,
            "NotCondition"       => JsonSerializer.Deserialize<NotCondition>(json, options)!,
            _ => throw new JsonException($"Unknown condition type: '{typeName}'")
        };
    }

    public override void Write(Utf8JsonWriter writer, ICondition value, JsonSerializerOptions options)
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
