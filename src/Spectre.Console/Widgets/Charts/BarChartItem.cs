namespace Spectre.Console;

/// <summary>
/// An item that's shown in a bar chart.
/// </summary>
public sealed class BarChartItem : IBarChartItem
{
    /// <summary>
    /// Gets the item label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the item value, or the sum of its segments.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the item color.
    /// </summary>
    public Color? Color { get; }

    /// <summary>
    /// Gets the segments of a stacked bar, or an empty list for a single-value bar.
    /// </summary>
    public IReadOnlyList<IBarChartItem> Segments { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="BarChartItem"/> class.
    /// </summary>
    /// <param name="label">The item label.</param>
    /// <param name="value">The item value.</param>
    /// <param name="color">The item color.</param>
    public BarChartItem(string label, double value, Color? color = null)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Value = value;
        Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarChartItem"/> class
    /// representing a stacked bar.
    /// </summary>
    /// <param name="label">The item label.</param>
    /// <param name="segments">The segments, in display order.</param>
    /// <remarks>
    /// Segment values must be finite and non-negative, and their sum must be finite.
    /// The segments are copied when the item is created.
    /// </remarks>
    public BarChartItem(string label, IEnumerable<IBarChartItem> segments)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        ArgumentNullException.ThrowIfNull(segments);

        var items = new List<IBarChartItem>();
        foreach (var item in segments)
        {
            ArgumentNullException.ThrowIfNull(item);

            var value = item.Value;
            if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(segments), "Segment values must be finite and non-negative.");
            }

            items.Add(new BarChartItem(item.Label, value, item.Color));
        }

        Segments = items.AsReadOnly();
        Value = items.Sum(item => item.Value);
        if (double.IsInfinity(Value))
        {
            throw new ArgumentOutOfRangeException(nameof(segments), "The sum of segment values must be finite.");
        }
    }
}