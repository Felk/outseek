using System;
using Outseek.AvaloniaClient.ViewModels;
using ReactiveUI;
using Range = Outseek.AvaloniaClient.Utils.Range;

namespace Outseek.AvaloniaClient.SharedViewModels;

public class ObservableRange : ViewModelBase
{
    private Range _range;
    private TimelineState TimelineState { get; }

    public ObservableRange(TimelineState timelineState, Range range)
    {
        TimelineState = timelineState;
        _range = range;

        // For lots of ranges it might be a bit insane for each individual range to register a listeners here.
        // It seems to not matter much right now, but if it does it might be better to have a dedicated
        // "ObservableRangesCollection" or something like that which manages all of its ranges together.
        timelineState
            .WhenAnyValue(t => t.DevicePixelsPerSecond)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(StepScaled));
                this.RaisePropertyChanged(nameof(RangeScaled));
            });
        timelineState
            .WhenAnyValue(t => t.Step)
            .Subscribe(_ => { this.RaisePropertyChanged(nameof(StepScaled)); });
    }

    public Range Range
    {
        get => _range;
        set
        {
            if (Equals(value, _range)) return;
            this.RaisePropertyChanging();
            this.RaisePropertyChanging(nameof(RangeScaled));
            _range = value;
            this.RaisePropertyChanged(nameof(RangeScaled));
            this.RaisePropertyChanged();
        }
    }

    public Range RangeScaled
    {
        get => Range * TimelineState.DevicePixelsPerSecond;
        set => Range = value / TimelineState.DevicePixelsPerSecond;
    }

    public double StepScaled
    {
        get => TimelineState.Step * TimelineState.DevicePixelsPerSecond;
        set => TimelineState.Step = value / TimelineState.DevicePixelsPerSecond;
    }
}
