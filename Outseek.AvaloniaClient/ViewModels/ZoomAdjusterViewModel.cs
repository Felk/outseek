using System;
using Avalonia;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI;
using Range = Outseek.AvaloniaClient.Utils.Range;

namespace Outseek.AvaloniaClient.ViewModels;

public class ZoomAdjusterViewModel : ViewModelBase
{
    public TimelineState TimelineState { get; }

    public Rect Bounds
    {
        set => TimelineState.ViewportWidth = value.Width;
    }

    private Range OffsetScaleToRange()
    {
        double range = TimelineState.End - TimelineState.Start;
        double scaledRange = range * TimelineState.ZoomScale;
        double from = (range - scaledRange) * TimelineState.ScrollOffset;

        return new Range(from, from + scaledRange);
    }

    private void RangeToOffsetScale(Range range)
    {
        double timelineRange = TimelineState.End - TimelineState.Start;

        double scale = range.Size / timelineRange;
        double rangeOffset = timelineRange - range.Size;
        double offset = rangeOffset == 0 ? 0 : range.From / rangeOffset;

        TimelineState.ScrollOffset = offset;
        TimelineState.ZoomScale = scale;
    }

    public Range Range
    {
        get => OffsetScaleToRange();
        set => RangeToOffsetScale(value);
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
            .Subscribe(_ => this.RaisePropertyChanged(nameof(Range)));
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
