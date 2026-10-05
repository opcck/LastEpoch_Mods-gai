using LastEpoch_Hud.Scripts.Core.Items;

namespace LastEpoch_Hud.Tests.Core.Items;

/// <summary>The forced legendary potential roll: whole number in the inclusive min..max range.</summary>
public sealed class LegendaryPotentialRollTests
{
    [Theory]
    [InlineData(0f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    public void Pick_SameMinMax_ReturnsThatValue(float fraction)
    {
        Assert.Equal(4, LegendaryPotentialRoll.Pick(4f, 4f, fraction));
    }

    [Fact]
    public void Pick_FractionZero_ReturnsMin()
    {
        Assert.Equal(0, LegendaryPotentialRoll.Pick(0f, 4f, 0f));
    }

    [Fact]
    public void Pick_FractionOne_ReturnsMax()
    {
        Assert.Equal(4, LegendaryPotentialRoll.Pick(0f, 4f, 1f));
    }

    [Fact]
    public void Pick_FractionJustBelowOne_ReturnsMax()
    {
        Assert.Equal(4, LegendaryPotentialRoll.Pick(0f, 4f, 0.99f));
    }

    [Theory]
    [InlineData(0.1f, 0)]
    [InlineData(0.3f, 1)]
    [InlineData(0.5f, 2)]
    [InlineData(0.7f, 3)]
    [InlineData(0.9f, 4)]
    public void Pick_SpreadsEvenlyOverInclusiveRange(float fraction, int expected)
    {
        Assert.Equal(expected, LegendaryPotentialRoll.Pick(0f, 4f, fraction));
    }

    [Theory]
    [InlineData(0.2f, 2)]
    [InlineData(0.5f, 3)]
    [InlineData(0.8f, 4)]
    public void Pick_NonZeroMin_SpreadsOverInclusiveRange(float fraction, int expected)
    {
        Assert.Equal(expected, LegendaryPotentialRoll.Pick(2f, 4f, fraction));
    }

    [Theory]
    [InlineData(0f, 1)]
    [InlineData(1f, 3)]
    public void Pick_FractionalSettings_Truncate(float fraction, int expected)
    {
        Assert.Equal(expected, LegendaryPotentialRoll.Pick(1.9f, 3.7f, fraction));
    }

    [Fact]
    public void Pick_MaxBelowMin_ReturnsMin()
    {
        Assert.Equal(3, LegendaryPotentialRoll.Pick(3f, 1f, 0.5f));
    }
}
