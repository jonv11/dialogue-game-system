namespace DialogueGameEngine.Persistence.Snapshot;

using DialogueGameEngine.Core;

public sealed record AttributeSnapshot
{
    public required AttributeAddress Address { get; init; }
    public required int Value { get; init; }
}
