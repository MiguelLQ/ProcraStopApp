using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace ProcraStop.Modules.Tasks.Presentation.Controls;

// editbycodex: control Skia ligero reutilizable para linea, dona y barras.
public sealed class TaskAnalyticsChart : SKCanvasView
{
    public static readonly BindableProperty ValuesProperty =
        BindableProperty.Create(nameof(Values), typeof(double[]), typeof(TaskAnalyticsChart),
            Array.Empty<double>(), propertyChanged: Invalidate);

    public static readonly BindableProperty LabelsProperty =
        BindableProperty.Create(nameof(Labels), typeof(string[]), typeof(TaskAnalyticsChart),
            Array.Empty<string>(), propertyChanged: Invalidate);

    public static readonly BindableProperty ChartTypeProperty =
        BindableProperty.Create(nameof(ChartType), typeof(string), typeof(TaskAnalyticsChart),
            "Line", propertyChanged: Invalidate);

    public double[] Values
    {
        get => (double[])GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public string[] Labels
    {
        get => (string[])GetValue(LabelsProperty);
        set => SetValue(LabelsProperty, value);
    }

    public string ChartType
    {
        get => (string)GetValue(ChartTypeProperty);
        set => SetValue(ChartTypeProperty, value);
    }

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((TaskAnalyticsChart)bindable).InvalidateSurface();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var values = Values ?? Array.Empty<double>();
        var labels = Labels ?? Array.Empty<string>();
        if (values.Length == 0)
        {
            DrawEmpty(canvas, e.Info);
            return;
        }

        switch (ChartType)
        {
            case "Donut":
                DrawDonut(canvas, e.Info, values, labels);
                break;
            case "Bars":
                DrawBars(canvas, e.Info, values, labels);
                break;
            default:
                DrawLine(canvas, e.Info, values, labels);
                break;
        }
    }

    private static void DrawLine(SKCanvas canvas, SKImageInfo info, double[] values, string[] labels)
    {
        const float left = 28;
        const float right = 18;
        const float top = 18;
        const float bottom = 42;
        var width = info.Width - left - right;
        var height = info.Height - top - bottom;
        var max = Math.Max(1, values.Max());

        using var grid = new SKPaint { Color = SKColor.Parse("#24364F"), StrokeWidth = 1, IsAntialias = true };
        using var line = new SKPaint { Color = SKColor.Parse("#4F8CFF"), StrokeWidth = 6, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
        using var fill = new SKPaint { Color = SKColor.Parse("#334F8CFF"), IsAntialias = true, Style = SKPaintStyle.Fill };
        using var point = new SKPaint { Color = SKColor.Parse("#7DB0FF"), IsAntialias = true };
        using var text = new SKPaint { Color = SKColor.Parse("#91A0B6"), TextSize = 22, IsAntialias = true, TextAlign = SKTextAlign.Center };

        for (var row = 0; row < 4; row++)
        {
            var y = top + height * row / 3f;
            canvas.DrawLine(left, y, left + width, y, grid);
        }

        var points = values.Select((value, index) => new SKPoint(
            values.Length == 1 ? left + width / 2 : left + width * index / (values.Length - 1f),
            top + height - (float)(value / max) * height)).ToArray();

        using var path = new SKPath();
        path.MoveTo(points[0]);
        for (var i = 1; i < points.Length; i++)
            path.LineTo(points[i]);
        canvas.DrawPath(path, line);

        using var area = new SKPath();
        area.MoveTo(points[0].X, top + height);
        area.LineTo(points[0]);
        for (var i = 1; i < points.Length; i++)
            area.LineTo(points[i]);
        area.LineTo(points[^1].X, top + height);
        area.Close();
        canvas.DrawPath(area, fill);

        for (var i = 0; i < points.Length; i++)
        {
            canvas.DrawCircle(points[i], 7, point);
            if (i < labels.Length)
                canvas.DrawText(labels[i], points[i].X, info.Height - 10, text);
        }
    }

    private static void DrawDonut(SKCanvas canvas, SKImageInfo info, double[] values, string[] labels)
    {
        var palette = new[] { "#34C77B", "#F4B740", "#4F8CFF", "#EF5B5B" };
        var total = values.Sum();
        var radius = Math.Min(info.Width, info.Height) * 0.28f;
        var center = new SKPoint(info.Width * 0.3f, info.Height * 0.5f);
        var rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
        var start = -90f;

        using var arc = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = radius * 0.42f, IsAntialias = true, StrokeCap = SKStrokeCap.Butt };
        using var centerValue = new SKPaint { Color = SKColor.Parse("#F4F7FB"), TextSize = 34, FakeBoldText = true, IsAntialias = true, TextAlign = SKTextAlign.Center };
        using var centerLabel = new SKPaint { Color = SKColor.Parse("#91A0B6"), TextSize = 20, IsAntialias = true, TextAlign = SKTextAlign.Center };
        using var legend = new SKPaint { Color = SKColor.Parse("#C7D0DE"), TextSize = 21, IsAntialias = true };

        if (total <= 0)
        {
            arc.Color = SKColor.Parse("#334155");
            canvas.DrawArc(rect, 0, 360, false, arc);
        }
        else
        {
            for (var i = 0; i < values.Length; i++)
            {
                var sweep = (float)(values[i] / total * 360);
                arc.Color = SKColor.Parse(palette[i % palette.Length]);
                canvas.DrawArc(rect, start, sweep, false, arc);
                start += sweep;
            }
        }

        canvas.DrawText(((int)total).ToString(), center.X, center.Y + 4, centerValue);
        canvas.DrawText("tareas", center.X, center.Y + 31, centerLabel);

        var legendX = info.Width * 0.58f;
        var legendY = info.Height * 0.28f;
        for (var i = 0; i < Math.Min(values.Length, labels.Length); i++)
        {
            using var dot = new SKPaint { Color = SKColor.Parse(palette[i % palette.Length]), IsAntialias = true };
            var y = legendY + i * 42;
            canvas.DrawCircle(legendX, y - 7, 7, dot);
            canvas.DrawText($"{labels[i]}  {(int)values[i]}", legendX + 18, y, legend);
        }
    }

    private static void DrawBars(SKCanvas canvas, SKImageInfo info, double[] values, string[] labels)
    {
        var palette = new[] { "#EF5B5B", "#F4B740", "#4F8CFF", "#34C77B" };
        var max = Math.Max(1, values.Max());
        var rowHeight = info.Height / (float)Math.Max(1, values.Length);
        using var label = new SKPaint { Color = SKColor.Parse("#C7D0DE"), TextSize = 22, IsAntialias = true };
        using var valuePaint = new SKPaint { Color = SKColor.Parse("#F4F7FB"), TextSize = 22, FakeBoldText = true, IsAntialias = true, TextAlign = SKTextAlign.Right };
        using var track = new SKPaint { Color = SKColor.Parse("#26364D"), IsAntialias = true };

        for (var i = 0; i < values.Length; i++)
        {
            var y = i * rowHeight + 12;
            var barTop = y + 32;
            var barWidth = info.Width - 34;
            var progress = (float)(values[i] / max);
            canvas.DrawText(i < labels.Length ? labels[i] : string.Empty, 12, y + 20, label);
            canvas.DrawText(((int)values[i]).ToString(), info.Width - 12, y + 20, valuePaint);
            canvas.DrawRoundRect(new SKRect(12, barTop, 12 + barWidth, barTop + 16), 8, 8, track);
            using var bar = new SKPaint { Color = SKColor.Parse(palette[i % palette.Length]), IsAntialias = true };
            canvas.DrawRoundRect(new SKRect(12, barTop, 12 + barWidth * progress, barTop + 16), 8, 8, bar);
        }
    }

    private static void DrawEmpty(SKCanvas canvas, SKImageInfo info)
    {
        using var paint = new SKPaint { Color = SKColor.Parse("#91A0B6"), TextSize = 24, IsAntialias = true, TextAlign = SKTextAlign.Center };
        canvas.DrawText("Aún no hay datos", info.Width / 2f, info.Height / 2f, paint);
    }
}
