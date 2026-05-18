namespace DialogueGameEngine.Cli.Commands;

using System.Text.Json;

internal sealed record CommandContext(JsonSerializerOptions SerializerOptions);

