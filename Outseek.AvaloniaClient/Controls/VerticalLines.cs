﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Outseek.AvaloniaClient.ViewModels.TimelineObjects;

namespace Outseek.AvaloniaClient.Controls
{
    public class VerticalLines : UserControl
    {
        public static readonly DirectProperty<VerticalLines, ObservableCollection<TimedTextIndicator>?>
            PositionsProperty =
                AvaloniaProperty.RegisterDirect<VerticalLines, ObservableCollection<TimedTextIndicator>?>(
                    nameof(Positions),
                    o => o.Positions,
                    (o, v) => o.Positions = v);

        public static readonly StyledProperty<Color> ColorProperty =
            AvaloniaProperty.Register<VerticalLines, Color>(nameof(Color), defaultValue: Colors.Red);

        public static readonly StyledProperty<double?> RenderFromProperty =
            AvaloniaProperty.Register<VerticalLines, double?>(nameof(RenderFrom));

        public static readonly StyledProperty<double?> RenderToProperty =
            AvaloniaProperty.Register<VerticalLines, double?>(nameof(RenderTo));

        private ObservableCollection<TimedTextIndicator>? _positions = new();

        public ObservableCollection<TimedTextIndicator>? Positions
        {
            get => _positions;
            set
            {
                if (_positions != null) _positions.CollectionChanged -= PositionsOnCollectionChanged;
                SetAndRaise(PositionsProperty, ref _positions, value);
                if (_positions != null) _positions.CollectionChanged += PositionsOnCollectionChanged;
            }
        }

        public Color Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public double? RenderFrom
        {
            get => GetValue(RenderFromProperty);
            set => SetValue(RenderFromProperty, value);
        }

        public double? RenderTo
        {
            get => GetValue(RenderToProperty);
            set => SetValue(RenderToProperty, value);
        }

        private void PositionsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateVisual();
        }

        static VerticalLines()
        {
            AffectsRender<VerticalLines>(PositionsProperty, ColorProperty, RenderFromProperty, RenderToProperty);
        }

        public override void Render(DrawingContext context)
        {
            if (Positions == null) return;

            int min = (int) Math.Max(RenderFrom ?? 0, Positions[0].AtScaled);
            int max = (int) Math.Ceiling(Math.Min(RenderTo ?? Bounds.Width, Positions[^1].AtScaled));
            int idx = 0;
            while (idx < Positions.Count && Positions[idx].AtScaled < min)
                idx++;
            int prevIdx = idx;

            for (int pixel = min;
                pixel < max && idx < Positions.Count;
                pixel = (int) Positions[idx].AtScaled)
            {
                while (idx < Positions.Count && (int) Positions[idx].AtScaled == pixel)
                    idx++;
                int count = idx - prevIdx;
                prevIdx = idx;
                double alpha = Math.Min(0.1 * count + 0.4, 1.0);
                uint color = ((uint) (0xff * alpha) << 24) + ((uint) Color.R << 16) + ((uint) Color.G << 8) + Color.B;
                context.DrawLine(new Pen(color), new Point(pixel, 0), new Point(pixel, DesiredSize.Height));
            }
        }
    }
}
