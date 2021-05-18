using System;
using Avalonia;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class ZoomAdjusterViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }

        public Rect Bounds
        {
            set => TimelineState.ViewportWidth = value.Width;
        }

        private (double, double) OffsetScaleToFromTo()
        {
            double range = TimelineState.End - TimelineState.Start;
            double scaledRange = range * TimelineState.ZoomScale;
            double from = (range - scaledRange) * TimelineState.ScrollOffset;

            return (from, from + scaledRange);
        }

        private void FromToToOffsetScale(double from, double to)
        {
            double range = TimelineState.End - TimelineState.Start;

            double scaledRange = to - from;
            double scale = scaledRange / range;
            double rangeOffset = range - scaledRange;
            double offset = rangeOffset == 0 ? 0 : from / rangeOffset;

            TimelineState.ScrollOffset = offset;
            TimelineState.ZoomScale = scale;
        }

        public double From
        {
            get => OffsetScaleToFromTo().Item1;
            set => FromToToOffsetScale(value, To);
        }

        public double To
        {
            get => OffsetScaleToFromTo().Item2;
            set => FromToToOffsetScale(From, value);
        }

        public double PlaybackIndicatorPosition =>
            TimelineState.PlaybackPosition / (TimelineState.End - TimelineState.Start) * TimelineState.ViewportWidth;

        public double HoverIndicatorPosition =>
            (TimelineState.ViewportHoverPosition + TimelineState.ScrollOffset * TimelineState.ScrollableWidth) /
            TimelineState.ViewportWidthScaled * TimelineState.ViewportWidth;

        public double MinimumDistance => TimelineState.Step * 100;

        public ZoomAdjusterViewModel(TimelineState timelineState)
        {
            TimelineState = timelineState;
            TimelineState
                .WhenAnyValue(t => t.ScrollOffset, t => t.ZoomScale, t => t.Start, t => t.End)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(From));
                    this.RaisePropertyChanged(nameof(To));
                });
            TimelineState
                .WhenAnyValue(t => t.Step)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(MinimumDistance)));
            TimelineState
                .WhenAnyValue(t => t.PlaybackPosition, t => t.End, t => t.Start, t => t.ViewportWidth)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(PlaybackIndicatorPosition)));
            TimelineState
                .WhenAnyValue(t => t.ViewportHoverPosition, t => t.ScrollOffset, t => t.ZoomScale, t => t.ViewportWidth)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HoverIndicatorPosition)));
        }

        public ZoomAdjusterViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }
    }
}
