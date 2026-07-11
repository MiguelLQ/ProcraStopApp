using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace ProcraStop.Modules.Dashboard.Presentation.Controls;

public sealed class DashboardBalanceChart : SKCanvasView
{
    public static readonly BindableProperty ValuesProperty =
        BindableProperty.Create(nameof(Values), typeof(double[]), typeof(DashboardBalanceChart),
            Array.Empty<double>(), propertyChanged: Invalidate);

    public static readonly BindableProperty LabelsProperty =
        BindableProperty.Create(nameof(Labels), typeof(string[]), typeof(DashboardBalanceChart),
            Array.Empty<string>(), propertyChanged: Invalidate);

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

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        ((DashboardBalanceChart)bindable).InvalidateSurface();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var values = Values ?? Array.Empty<double>();
        var labels = Labels ?? Array.Empty<string>();
        if (values.Length < 3)
        {
            DrawEmpty(canvas, e.Info);
            return;
        }

        DrawRadar(canvas, e.Info, values.Take(3).ToArray(), labels.Take(3).ToArray());
    }

    private static void DrawRadar(SKCanvas canvas, SKImageInfo info, double[] values, string[] labels)
    {
        var center = new SKPoint(info.Width / 2f, info.Height * 0.5f);
        var radius = Math.Min(info.Width, info.Height) * 0.31f;
        var palette = new[] { "#4F8CFF", "#34C77B", "#EF5B5B" };

        using var grid = new SKPaint { Color = SKColor.Parse("#2A3648"), StrokeWidth = 2, IsAntialias = true, Style = SKPaintStyle.Stroke };
        using var axis = new SKPaint { Color = SKColor.Parse("#334155"), StrokeWidth = 2, IsAntialias = true };
        using var fill = new SKPaint { Color = SKColor.Parse("#334F8CFF"), IsAntialias = true, Style = SKPaintStyle.Fill };
        using var outline = new SKPaint { Color = SKColor.Parse("#7DB0FF"), StrokeWidth = 4, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeJoin = SKStrokeJoin.Round };
        using var labelPaint = new SKPaint { Color = SKColor.Parse("#C7D0DE"), TextSize = 22, IsAntialias = true, TextAlign = SKTextAlign.Center };
        using var valuePaint = new SKPaint { Color = SKColor.Parse("#F4F7FB"), TextSize = 20, FakeBoldText = true, IsAntialias = true, TextAlign = SKTextAlign.Center };

        for (var ring = 1; ring <= 3; ring++)
            canvas.DrawPath(BuildPolygon(center, radius * ring / 3f, 3), grid);

        var points = new List<SKPoint>();
        for (var i = 0; i < 3; i++)
        {
            var angle = -Math.PI / 2 + i * Math.PI * 2 / 3;
            var outer = PointOnCircle(center, radius, angle);
            canvas.DrawLine(center, outer, axis);

            var value = Math.Clamp(values[i], 0, 1);
            points.Add(PointOnCircle(center, radius * (float)value, angle));

            var labelPoint = PointOnCircle(center, radius + 38, angle);
            if (i < labels.Length)
                canvas.DrawText(labels[i], labelPoint.X, labelPoint.Y + 8, labelPaint);
        }

        using var area = new SKPath();
        area.MoveTo(points[0]);
        area.LineTo(points[1]);
        area.LineTo(points[2]);
        area.Close();
        canvas.DrawPath(area, fill);
        canvas.DrawPath(area, outline);

        for (var i = 0; i < points.Count; i++)
        {
            using var dot = new SKPaint { Color = SKColor.Parse(palette[i]), IsAntialias = true };
            canvas.DrawCircle(points[i], 8, dot);
            canvas.DrawText($"{values[i]:P0}", points[i].X, points[i].Y - 14, valuePaint);
        }
    }

    private static SKPath BuildPolygon(SKPoint center, float radius, int sides)
    {
        var path = new SKPath();
        for (var i = 0; i < sides; i++)
        {
            var angle = -Math.PI / 2 + i * Math.PI * 2 / sides;
            var point = PointOnCircle(center, radius, angle);
            if (i == 0)
                path.MoveTo(point);
            else
                path.LineTo(point);
        }

        path.Close();
        return path;
    }

    private static SKPoint PointOnCircle(SKPoint center, float radius, double angle)
    {
        return new SKPoint(
            center.X + radius * (float)Math.Cos(angle),
            center.Y + radius * (float)Math.Sin(angle));
    }

    private static void DrawEmpty(SKCanvas canvas, SKImageInfo info)
    {
        using var paint = new SKPaint { Color = SKColor.Parse("#91A0B6"), TextSize = 24, IsAntialias = true, TextAlign = SKTextAlign.Center };
        canvas.DrawText("Sin señales suficientes", info.Width / 2f, info.Height / 2f, paint);
    }
}
