using DialogueGameEngine.Core.Tests.Helpers;

namespace DialogueGameEngine.Core.Tests;

public class AttributeValueTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(50, 50)]
    [InlineData(-50, -50)]
    [InlineData(100, 100)]
    [InlineData(-100, -100)]
    [InlineData(101, 100)]
    [InlineData(-101, -100)]
    [InlineData(9999, 100)]
    [InlineData(-9999, -100)]
    public void Constructor_ClampsValueToRange(int input, int expected)
    {
        var value = new AttributeValue(input);

        ((int)value).Should().Be(expected);
    }

    [Fact]
    public void Constructor_WithinRange_StaysExact()
    {
        var value = new AttributeValue(42);

        ((int)value).Should().Be(42);
    }

    [Fact]
    public void Constructor_AboveMax_ClampsTo100()
    {
        var value = new AttributeValue(150);

        ((int)value).Should().Be(AttributeValue.Max);
    }

    [Fact]
    public void Constructor_BelowMin_ClampToNeg100()
    {
        var value = new AttributeValue(-150);

        ((int)value).Should().Be(AttributeValue.Min);
    }

    [Theory]
    [InlineData(50, 10, 60)]
    [InlineData(50, -10, 40)]
    [InlineData(95, 10, 100)]
    [InlineData(-95, -10, -100)]
    [InlineData(100, 1, 100)]
    [InlineData(-100, -1, -100)]
    [InlineData(0, 200, 100)]
    [InlineData(0, -200, -100)]
    public void Add_ClampsResult(int initial, int delta, int expected)
    {
        var value = new AttributeValue(initial);

        var result = value.Add(delta);

        ((int)result).Should().Be(expected);
    }

    [Fact]
    public void ImplicitIntConversion_ReturnsCorrectValue()
    {
        var value = new AttributeValue(77);

        int converted = value;

        converted.Should().Be(77);
    }

    [Theory]
    [InlineData(AttributeValue.Min)]
    [InlineData(AttributeValue.Max)]
    [InlineData(0)]
    public void Constructor_BoundaryValues_StayExact(int boundary)
    {
        var value = new AttributeValue(boundary);

        ((int)value).Should().Be(boundary);
    }
}
