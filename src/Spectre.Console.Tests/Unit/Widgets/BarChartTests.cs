namespace Spectre.Console.Tests.Unit;

[ExpectationPath("Widgets/BarChart")]
public sealed class BarChartTests
{
    [Fact]
    [Expectation("Render")]
    public async Task Should_Render_Correctly()
    {
        // Given
        var console = new TestConsole();

        // When
        console.Write(new BarChart()
            .Width(60)
            .Label("Number of fruits")
            .AddItem("Apple", 12)
            .AddItem("Orange", 54)
            .AddItem("Banana", 33));

        // Then
        await Verifier.Verify(console.Output);
    }

    [Fact]
    [Expectation("Zero_Value")]
    public async Task Should_Render_Correctly_2()
    {
        // Given
        var console = new TestConsole();

        // When
        console.Write(new BarChart()
            .Width(60)
            .Label("Number of fruits")
            .AddItem("Apple", 0)
            .AddItem("Orange", 54)
            .AddItem("Banana", 33));

        // Then
        await Verifier.Verify(console.Output);
    }

    [Fact]
    [Expectation("Fixed_Max_Value")]
    public async Task Should_Render_Correctly_3()
    {
        // Given
        var console = new TestConsole();

        // When
        console.Write(new BarChart()
            .Width(60)
            .WithMaxValue(100)
            .Label("Number of fruits")
            .AddItem("Apple", 12)
            .AddItem("Orange", 54)
            .AddItem("Banana", 33));

        // Then
        await Verifier.Verify(console.Output);
    }

    [Theory]
    [InlineData(33, 1, 2, 10, 20)]
    [InlineData(33, 0.001, 0.002, 10, 20)]
    [InlineData(33, 0, 1, 0, 30)]
    [InlineData(33, 1, 0, 30, 0)]
    [InlineData(33, 0, 0, 0, 0)]
    [InlineData(8, 1, 1, 3, 2)]
    public void Should_Render_Proportional_Colored_Segments(int width, double firstValue, double secondValue, int firstWidth, int secondWidth)
    {
        // Given
        var console = new TestConsole();
        var chart = new BarChart()
            .Width(width)
            .HideValues()
            .AddItem(new BarChartItem("A",
            [
                new BarChartItem("First", firstValue, Color.Red),
                new BarChartItem("Second", secondValue, Color.Blue),
            ]));

        // When
        var segments = chart.GetSegments(console).ToArray();

        // Then
        segments.Where(segment => segment.Style.Foreground == Color.Red).Sum(segment => segment.CellCount()).ShouldBe(firstWidth);
        segments.Where(segment => segment.Style.Foreground == Color.Blue).Sum(segment => segment.CellCount()).ShouldBe(secondWidth);
    }

    [Theory]
    [InlineData(false, 0, 10, 20, 5)]
    [InlineData(false, 8, 5, 10, 2)]
    [InlineData(true, 8, 20, 20, 20)]
    public void Should_Scale_Bars_And_Handle_Empty_Stacks(bool percentage, double maxValue, int firstWidth, int secondWidth, int singleWidth)
    {
        // Given
        var console = new TestConsole();
        var chart = new BarChart()
            .Width(23)
            .HideValues()
            .UsePercentageScale(percentage)
            .WithMaxValue(maxValue)
            .AddStackedItem("A", new BarChartItem("First", 0.5), new BarChartItem("Second", 1.5))
            .AddStackedItem("B", new BarChartItem("First", 1), new BarChartItem("Second", 3))
            .AddStackedItem("C", new BarChartItem("First", 0), new BarChartItem("Second", 0))
            .AddStackedItem("D")
            .AddItem("E", 1);

        // When
        console.Write(chart);

        // Then
        console.Lines.Select(line => line.Count(character => character == '\u2588'))
            .ShouldBe([firstWidth, secondWidth, 0, 0, singleWidth]);
    }

    [Theory]
    [InlineData(false, 8, 40)]
    [InlineData(true, 40, 40)]
    public void Should_Reserve_The_Same_Bar_Width_For_Different_Length_Totals(bool percentage, int firstWidth, int secondWidth)
    {
        // Given
        var console = new TestConsole();
        var chart = new BarChart()
            .Width(46)
            .UsePercentageScale(percentage)
            .AddStackedItem("A", new BarChartItem("First", 1), new BarChartItem("Second", 1))
            .AddStackedItem("B", new BarChartItem("First", 5), new BarChartItem("Second", 5));

        // When
        console.Write(chart);

        // Then
        console.Lines.Select(line => line.Count(character => character == '\u2588'))
            .ShouldBe([firstWidth, secondWidth]);
    }

    [Fact]
    [Expectation("Stacked_Percentage")]
    public async Task Should_Render_Percentages_And_A_Shared_Legend()
    {
        // Given
        var console = new TestConsole();
        var chart = new BarChart()
            .Width(60)
            .Label("Workload")
            .UsePercentageScale()
            .ShowSegmentValues()
            .ShowLegend()
            .AddStackedItem("Q1",
                new BarChartItem("Completed [h]", 30, Color.Green),
                new BarChartItem("Remaining", 10, Color.Yellow))
            .AddStackedItem("Q2",
                new BarChartItem("Completed [h]", 5, Color.Green),
                new BarChartItem("Remaining", 15, Color.Yellow))
            .AddStackedItem("Q3",
                new BarChartItem("Completed [h]", 0, Color.Green),
                new BarChartItem("Remaining", 0, Color.Yellow));

        // When
        console.Write(chart);

        // Then
        await Verifier.Verify(console.Output);
    }

    [Fact]
    public void Should_Format_Inline_Values_Using_The_Chart_Culture()
    {
        // Given
        var console = new TestConsole();
        var chart = new BarChart()
            .ShowSegmentValues()
            .Culture("de-DE")
            .UseValueFormatter((value, culture) => value.ToString("0.#", culture) + " kg")
            .AddStackedItem("A",
                new BarChartItem("First", 1.5, Color.Yellow),
                new BarChartItem("Second", 2.5, Color.Blue));

        // When
        var segments = chart.GetSegments(console).ToArray();

        // Then
        var first = segments.Single(segment => segment.Text == "1,5 kg");
        first.Style.Background.ShouldBe(Color.Yellow);
        first.Style.Foreground.ShouldBe(Color.Black);
        var second = segments.Single(segment => segment.Text == "2,5 kg");
        second.Style.Background.ShouldBe(Color.Blue);
        second.Style.Foreground.ShouldBe(Color.White);
        string.Concat(segments.Select(segment => segment.Text)).ShouldContain("4 kg");
    }

    [Fact]
    public void Should_Format_Percentages_Without_Changing_The_Total()
    {
        // Given
        var console = new TestConsole();
        var chart = new BarChart()
            .UsePercentageScale()
            .ShowSegmentValues()
            .Culture("de-DE")
            .UseValueFormatter((value, culture) => value.ToString("0.#", culture) + " h")
            .AddStackedItem("A",
                new BarChartItem("First", 0.5, Color.Red),
                new BarChartItem("Second", 3.5, Color.Blue));

        // When
        console.Write(chart);

        // Then
        console.Output.ShouldContain("12,5%");
        console.Output.ShouldContain("87,5%");
        console.Output.ShouldContain("4 h");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(12)]
    public void Should_Omit_Inline_Labels_That_Do_Not_Fit(int width)
    {
        // Given
        var console = new TestConsole();
        console.Profile.Width = width;
        var chart = new BarChart()
            .UsePercentageScale()
            .ShowSegmentValues()
            .AddStackedItem("A",
                new BarChartItem("First", 1, Color.Red),
                new BarChartItem("Second", 999999, Color.Blue));

        // When
        console.Write(chart);

        // Then
        console.Lines.ShouldAllBe(line => Cell.GetCellLength(line) <= width);
        console.Output.ShouldNotContain("%");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Should_Reject_Invalid_Stacked_Values(double value)
    {
        // Given
        var chart = new BarChart();

        // When, Then
        Should.Throw<ArgumentOutOfRangeException>(() => chart.AddStackedItem("A", new BarChartItem("First", value)));
    }

    [Fact]
    public void Should_Reject_An_Overflowing_Stacked_Total()
    {
        // Given
        var chart = new BarChart();

        // When, Then
        Should.Throw<ArgumentOutOfRangeException>(() => chart.AddStackedItem("A",
            new BarChartItem("First", double.MaxValue),
            new BarChartItem("Second", double.MaxValue)));
    }
}