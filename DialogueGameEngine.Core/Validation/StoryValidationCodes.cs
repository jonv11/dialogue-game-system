namespace DialogueGameEngine.Core;

/// <summary>Stable issue codes emitted by story loading and validation.</summary>
public static class StoryValidationCodes
{
    public const string InvalidJson = "STORY001";
    public const string MissingRequiredField = "STORY002";
    public const string DuplicateSceneId = "STORY003";
    public const string DuplicateChoiceId = "STORY004";
    public const string UnknownSceneReference = "STORY005";
    public const string MissingStartScene = "STORY006";
    public const string UnreachableScene = "STORY007";
    public const string InvalidAttributeAddress = "STORY008";
    public const string UnknownConditionType = "STORY009";
    public const string UnknownEffectType = "STORY010";
    public const string InvalidInitialState = "STORY011";
    public const string NoPlayablePath = "STORY012";
    public const string InvalidLifecycleEffect = "STORY013";
    public const string StoryPathNotFound = "STORY014";
    public const string InvalidIdentifier = "STORY015";
    public const string AttributeValueOutOfRange = "STORY016";
    public const string InvalidConditionPayload = "STORY017";
    public const string InvalidEffectPayload = "STORY018";
}

