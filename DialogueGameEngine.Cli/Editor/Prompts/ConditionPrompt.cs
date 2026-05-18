namespace DialogueGameEngine.Cli.Editor.Prompts;

using DialogueGameEngine.Core;
using Spectre.Console;

internal static class ConditionPrompt
{
    private enum ConditionType
    {
        None,
        Attribute,
        Flag,
        All,
        Any,
        Not,
    }

    internal static ICondition? Ask(string current = "none")
    {
        AnsiConsole.MarkupLine($"[dim]Current: {Markup.Escape(current)}[/]");

        var type = AnsiConsole.Prompt(
            new SelectionPrompt<ConditionType>()
                .Title("Condition type:")
                .AddChoices(Enum.GetValues<ConditionType>())
                .UseConverter(t => t switch
                {
                    ConditionType.None      => "(no condition — always available)",
                    ConditionType.Attribute => "AttributeCondition — compare an attribute value",
                    ConditionType.Flag      => "FlagCondition      — check if a flag is set",
                    ConditionType.All       => "AllCondition       — AND: all must be met",
                    ConditionType.Any       => "AnyCondition       — OR: at least one must be met",
                    ConditionType.Not       => "NotCondition       — NOT: negate a condition",
                    _                       => t.ToString()
                }));

        return type switch
        {
            ConditionType.None      => null,
            ConditionType.Attribute => AskAttributeCondition(),
            ConditionType.Flag      => AskFlagCondition(),
            ConditionType.All       => new AllCondition { Conditions = AskList("AND — add conditions (all must be met):") },
            ConditionType.Any       => new AnyCondition { Conditions = AskList("OR — add conditions (at least one must be met):") },
            ConditionType.Not       => AskNotCondition(),
            _                       => null
        };
    }

    internal static string Describe(ICondition? condition) => condition switch
    {
        null                  => "none",
        AttributeCondition c  => $"Attr: {AttributeAddressPrompt.Format(c.Target)} {OperatorSymbol(c.Operator)} {c.Value}",
        FlagCondition c       => $"Flag: {c.Flag.Value} {(c.Expected ? "= set" : "= not set")}",
        AllCondition c        => $"All ({c.Conditions.Count})",
        AnyCondition c        => $"Any ({c.Conditions.Count})",
        NotCondition c        => $"Not ({Describe(c.Condition)})",
        _                     => condition.GetType().Name
    };

    private static AttributeCondition AskAttributeCondition()
    {
        var target = AttributeAddressPrompt.Ask("Attribute to compare:");

        var op = AnsiConsole.Prompt(
            new SelectionPrompt<ComparisonOperator>()
                .Title("Operator:")
                .AddChoices(Enum.GetValues<ComparisonOperator>())
                .UseConverter(o => o switch
                {
                    ComparisonOperator.LessThan           => "<   Less than",
                    ComparisonOperator.LessThanOrEqual    => "<=  Less than or equal",
                    ComparisonOperator.Equal              => "==  Equal to",
                    ComparisonOperator.GreaterThanOrEqual => ">=  Greater than or equal",
                    ComparisonOperator.GreaterThan        => ">   Greater than",
                    _                                     => o.ToString()
                }));

        var value        = AnsiConsole.Prompt(new TextPrompt<int>("Threshold value (−100 to 100):"));
        var useEffective = AnsiConsole.Confirm("Use effective value (includes modifier deltas)?", defaultValue: true);

        return new AttributeCondition
        {
            Target           = target,
            Operator         = op,
            Value            = value,
            UseEffectiveValue = useEffective
        };
    }

    private static FlagCondition AskFlagCondition()
    {
        var flag     = AnsiConsole.Prompt(new TextPrompt<string>("Flag name (PastTense convention):"));
        var expected = AnsiConsole.Confirm("Must the flag be set?", defaultValue: true);
        return new FlagCondition { Flag = new FlagId(flag), Expected = expected };
    }

    private static NotCondition AskNotCondition()
    {
        AnsiConsole.MarkupLine("Define the condition to negate:");
        var inner = Ask() ?? new FlagCondition { Flag = new FlagId("Placeholder"), Expected = true };
        return new NotCondition { Condition = inner };
    }

    private static IReadOnlyList<ICondition> AskList(string title)
    {
        AnsiConsole.MarkupLine(title);
        var list = new List<ICondition>();

        while (AnsiConsole.Confirm(list.Count == 0 ? "Add a condition?" : "Add another?", defaultValue: true))
        {
            var c = Ask();
            if (c is not null)
                list.Add(c);
        }

        return list;
    }

    private static string OperatorSymbol(ComparisonOperator op) => op switch
    {
        ComparisonOperator.LessThan           => "<",
        ComparisonOperator.LessThanOrEqual    => "<=",
        ComparisonOperator.Equal              => "==",
        ComparisonOperator.GreaterThanOrEqual => ">=",
        ComparisonOperator.GreaterThan        => ">",
        _                                     => "?"
    };
}
