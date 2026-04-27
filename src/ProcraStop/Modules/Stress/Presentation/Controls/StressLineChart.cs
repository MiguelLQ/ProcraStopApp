
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace ProcraStop.Modules.Stress.Presentation.Controls
{
    public class StressLineChart : SKCanvasView
    {
        // Bindable: valores Y
        public static readonly BindableProperty ValuesProperty =
            BindableProperty.Create(nameof(Values), typeof(double[]), typeof(StressLineChart),
                Array.Empty<double>(), propertyChanged: (b, _, __) => ((StressLineChart)b).InvalidateSurface());

        // Bindable: etiquetas X
        public static readonly BindableProperty LabelsProperty =
            BindableProperty.Create(nameof(Labels), typeof(string[]), typeof(StressLineChart),
                Array.Empty<string>(), propertyChanged: (b, _, __) => ((StressLineChart)b).InvalidateSurface());

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

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface?.Canvas;
            if (canvas == null) return;
            canvas.Clear(SKColors.Transparent);
            var values = Values;
            var labels = Labels;
            if (values == null || values.Length < 2) return;

            var info = e.Info;
            float padL = 55f, padR = 20f, padT = 20f, padB = 45f;
            float width = info.Width - padL - padR;
            float height = info.Height - padT - padB;

            double minVal = 0, maxVal = 1; // fijo para estrés 0-1

            SKPoint[] points = new SKPoint[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                float x = padL + (i / (float)(values.Length - 1)) * width;
                float y = padT + (float)((maxVal - values[i]) / (maxVal - minVal)) * height;
                points[i] = new SKPoint(x, y);
            }

            // ── Grid horizontal + etiquetas Y ──────────────────────────
            using var gridPaint = new SKPaint { Color = SKColor.Parse("#E5E7EB"), StrokeWidth = 1, IsAntialias = true };
            using var labelPaint = new SKPaint { Color = SKColor.Parse("#6B7280"), TextSize = 24, IsAntialias = true, TextAlign = SKTextAlign.Right };

            float[] yTicks = { 0f, 0.25f, 0.5f, 0.75f, 1f };
            foreach (var tick in yTicks)
            {
                float y = padT + (float)(1 - tick) * height;
                canvas.DrawLine(padL, y, padL + width, y, gridPaint);
                canvas.DrawText(tick.ToString("F2"), padL - 6, y + 8, labelPaint);
            }

            // ── Línea de promedio (dashed) ──────────────────────────────
            double avg = values.Average();
            float avgY = padT + (float)((maxVal - avg) / (maxVal - minVal)) * height;
            using var avgPaint = new SKPaint
            {
                Color = SKColor.Parse("#B4B2A9"),
                StrokeWidth = 2,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 12, 6 }, 0)
            };
            canvas.DrawLine(padL, avgY, padL + width, avgY, avgPaint);

            // ── Área rellena ────────────────────────────────────────────
            var fillPath = new SKPath();
            fillPath.MoveTo(points[0].X, padT + height);
            foreach (var p in points) fillPath.LineTo(p.X, p.Y);
            fillPath.LineTo(points[^1].X, padT + height);
            fillPath.Close();

            using var fillPaint = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, padT),
                    new SKPoint(0, padT + height),
                    new[] { SKColor.Parse("#CC4075ff"), SKColor.Parse("#0D070BF2") },
                    SKShaderTileMode.Clamp)
            };
            canvas.DrawPath(fillPath, fillPaint);

            // ── Línea principal (suavizada con cubics) ──────────────────
            using var linePaint = new SKPaint
            {
                Color = SKColor.Parse("#4075ff"),
                StrokeWidth = 3,
                IsStroke = true,
                IsAntialias = true,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeCap = SKStrokeCap.Round
            };
            var linePath = new SKPath();
            linePath.MoveTo(points[0]);
            for (int i = 1; i < points.Length; i++)
            {
                float cpX = (points[i - 1].X + points[i].X) / 2;
                linePath.CubicTo(cpX, points[i - 1].Y, cpX, points[i].Y, points[i].X, points[i].Y);
            }
            canvas.DrawPath(linePath, linePaint);

            // ── Puntos con color por nivel ──────────────────────────────
            using var borderDot = new SKPaint { Color = SKColors.White, IsAntialias = true };
            for (int i = 0; i < points.Length; i++)
            {
                // Rojo = alto, verde = bajo, ámbar = medio
                var color = values[i] >= 0.75 ? SKColor.Parse("#E24B4A")
                          : values[i] <= 0.35 ? SKColor.Parse("#1D9E75")
                          : SKColor.Parse("#EF9F27");

                using var dotPaint = new SKPaint { Color = color, IsAntialias = true };
                canvas.DrawCircle(points[i], 9, borderDot);
                canvas.DrawCircle(points[i], 6, dotPaint);
            }

            // ── Etiquetas X ─────────────────────────────────────────────
            if (labels != null && labels.Length == values.Length)
            {
                using var xPaint = new SKPaint
                {
                    Color = SKColor.Parse("#6B7280"),
                    TextSize = 24,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                for (int i = 0; i < labels.Length; i++)
                    canvas.DrawText(labels[i], points[i].X, padT + height + 35, xPaint);
            }
        }
    }
}
