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
            double offset = from / (range - scaledRange);

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
        }

        public ZoomAdjusterViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }
    }
}
