namespace Spectre.Console;

internal sealed class BarChartBar : Renderable
{
    private readonly BarChart _chart;
    private readonly IBarChartItem _item;
    private readonly double _maxValue;
    private readonly int _valueWidth;

    public BarChartBar(BarChart chart, IBarChartItem item, double maxValue, int valueWidth)
    {
        _chart = chart;
        _item = item;
        _maxValue = maxValue;
        _valueWidth = valueWidth;
    }

    protected override Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(0, maxWidth);
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var culture = _chart.Culture ?? CultureInfo.InvariantCulture;
        var total = Math.Max(0, _item.Value);
        var available = Math.Max(0, maxWidth - (_chart.ShowValues ? _valueWidth + 1 : 0));
        var scale = _chart.UsePercentageScale ? total : _maxValue;
        var barWidth = scale > 0
            ? Math.Max(0, (int)(available * (total / scale)))
            : 0;

        IReadOnlyList<IBarChartItem> items = _item is BarChartItem barChartItem && barChartItem.Segments.Count > 0
            ? barChartItem.Segments
            : [_item];

        var cumulative = 0d;
        var rendered = 0;
        foreach (var item in items)
        {
            var value = Math.Max(0, item.Value);
            cumulative += value;

            // Round cumulative boundaries so segment widths add up to the bar width.
            var end = barWidth > 0
                ? Math.Min(barWidth, (int)Math.Round(barWidth * (cumulative / total), MidpointRounding.AwayFromZero))
                : 0;
            var width = Math.Max(0, end - rendered);
            rendered += width;
            if (width == 0)
            {
                continue;
            }

            var color = item.Color ?? Color.Default;
            var style = new Style(foreground: color);
            var label = _chart.ShowSegmentValues
                ? _chart.UsePercentageScale
                    ? (value / total).ToString("0.#%", culture)
                    : FormatValue(value, culture)
                : string.Empty;
            var labelWidth = Cell.GetCellLength(label);
            if (labelWidth == 0 || labelWidth > width)
            {
                yield return new Segment(new string('█', width), style);
                continue;
            }

            var left = (width - labelWidth) / 2;
            var foreground = color == Color.Default
                ? Color.Default
                : (color.R * 299) + (color.G * 587) + (color.B * 114) >= 128000
                    ? Color.Black
                    : Color.White;

            yield return new Segment(new string('█', left), style);
            yield return new Segment(label, new Style(foreground: foreground, background: color));
            yield return new Segment(new string('█', width - left - labelWidth), style);
        }

        if (_chart.ShowValues)
        {
            var value = new Segment(
                (barWidth > 0 ? " " : string.Empty) + FormatValue(total, culture),
                new Style(foreground: _item.Color ?? Color.Default));
            var segment = Segment.Truncate(value, Math.Max(0, maxWidth - rendered));
            if (segment != null)
            {
                yield return segment;
                rendered += segment.CellCount();
            }
        }

        if (rendered < maxWidth)
        {
            yield return Segment.Padding(maxWidth - rendered);
        }
    }

    private string FormatValue(double value, CultureInfo culture)
    {
        return _chart.ValueFormatter != null
            ? _chart.ValueFormatter(value, culture)
            : value.ToString(culture);
    }
}