using Spectre.Console;

var chart = new BarChart()
    .Width(80)
    .Label("[b u]Fruit Sales[/]")
    .AddItem("Apples", 10, Color.Red)
    .AddItem("Bananas", 5, Color.Yellow)
    .AddItem("Cherries", 8, Color.Purple);

AnsiConsole.Write(chart);
AnsiConsole.WriteLine();

var stacked = new BarChart()
    .Width(80)
    .Label("[b u]Fruit Sales[/]")
    .ShowLegend()
    .HideValues()
    .ShowSegmentValues()
    .AddStackedItem("Apples", new BarChartItem("Granny Smith", 5, Color.Green),
        new BarChartItem("Red Delicious", 3, Color.Red),
        new BarChartItem("Golden Delicious", 2, Color.Yellow))
    .AddItem("Bananas", 5, Color.Yellow)
    .AddItem("Cherries", 8, Color.Purple);

AnsiConsole.Write(stacked);
AnsiConsole.WriteLine();

var percentage = new BarChart()
    .Width(80)
    .Label("[b u]Fruit Sales[/]")
    .ShowLegend()
    .ShowSegmentValues()
    .UsePercentageScale()
    .AddStackedItem("Apples", new BarChartItem("Granny Smith", 5, Color.Green),
        new BarChartItem("Red Delicious", 3, Color.Red),
        new BarChartItem("Golden Delicious", 2, Color.Yellow))
    .AddItem("Bananas", 5, Color.Yellow)
    .AddItem("Cherries", 8, Color.Purple);

AnsiConsole.Write(percentage);
