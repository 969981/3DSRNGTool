using Pk3DSRNGTool.Core;
using Xunit;

namespace Pk3DSRNGTool.Tests;

public class ShinyFilterTests
{
    [Fact]
    public void Shiny_filter_defaults_to_any()
    {
        Assert.Equal(ShinyFilterMode.Any, new RNGFilters().ShinyFilter);
    }

    [Theory]
    [InlineData(ShinyFilterMode.Any, true, true, true)]
    [InlineData(ShinyFilterMode.NoShiny, true, false, false)]
    [InlineData(ShinyFilterMode.StarOnly, false, true, false)]
    [InlineData(ShinyFilterMode.SquareOnly, false, false, true)]
    [InlineData(ShinyFilterMode.StarOrSquare, false, true, true)]
    public void Shiny_filter_mode_selects_only_the_requested_shiny_results(
        ShinyFilterMode mode,
        bool expectedNonShiny,
        bool expectedStarShiny,
        bool expectedSquareShiny)
    {
        var filters = CreateFilters();
        filters.ShinyFilter = mode;

        Assert.Equal(expectedNonShiny, filters.CheckResult(CreateResult(shiny: false, squareShiny: false)));
        Assert.Equal(expectedStarShiny, filters.CheckResult(CreateResult(shiny: true, squareShiny: false)));
        Assert.Equal(expectedSquareShiny, filters.CheckResult(CreateResult(shiny: true, squareShiny: true)));
    }

    [Theory]
    [InlineData(false, false, "")]
    [InlineData(true, false, "★")]
    [InlineData(true, true, "◆")]
    public void Shiny_display_uses_the_expected_symbol(bool shiny, bool squareShiny, string expected)
    {
        Assert.Equal(expected, CreateResult(shiny, squareShiny).ShinyDisplay);
    }

    private static RNGFilters CreateFilters() => new()
    {
        Nature = new bool[25],
        HPType = new bool[18],
        IVlow = new int[6],
        IVup = new[] { 31, 31, 31, 31, 31, 31 },
        PerfectIVValue = 31,
        PerfectIVCount = 0,
    };

    private static RNGResult CreateResult(bool shiny, bool squareShiny) => new TestResult
    {
        IVs = new[] { 31, 31, 31, 31, 31, 31 },
        Nature = 0,
        Shiny = shiny,
        SquareShiny = squareShiny,
    };

    private sealed class TestResult : RNGResult
    {
    }
}
