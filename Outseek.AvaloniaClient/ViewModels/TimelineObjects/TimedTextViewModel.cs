using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects;

public class TimedTextViewModel : TimelineObjectViewModelBase
{
    private readonly TimelineObject.TimedText _timedText;
    public TimelineState TimelineState { get; }

    public ObservableCollection<TimedTextIndicator> Indicators { get; } = new();

    public TimedTextViewModel(TimelineState timelineState, TimelineObject.TimedText timedText)
    {
        TimelineState = timelineState;
        _timedText = timedText;
        TimelineState
            .WhenAnyValue(t => t.ScrollOffset, t => t.ViewportWidth, t => t.ViewportWidthScaled)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ScrollMin));
                this.RaisePropertyChanged(nameof(ScrollMax));
            });
    }

    public double ScrollMin => TimelineState.ScrollOffset *
                               (TimelineState.ViewportWidthScaled - TimelineState.ViewportWidth);

    public double ScrollMax => ScrollMin + TimelineState.ViewportWidth;

    public TimedTextViewModel() : this(
        new TimelineState(), new TimelineObject.TimedText(AsyncEnumerable.Empty<TimedTextEntry>))
    {
        // the default constructor is only used by the designer
    }

    public override async Task Refresh(CancellationToken cancellationToken)
    {
        Indicators.Clear();
        await foreach (TimedTextEntry entry in _timedText.Entries().WithCancellation(cancellationToken))
        {
            var vm = new TimedTextIndicator(TimelineState, entry.FromSeconds);
            Dispatcher.UIThread.Post(() => Indicators.Add(vm));
        }
    }
}

public class TimedTextIndicator : ViewModelBase
{
    private TimelineState TimelineState { get; }
    private readonly double _at;

    public TimedTextIndicator(TimelineState timelineState, double at)
    {
        TimelineState = timelineState;
        _at = at;
    }

    public double AtScaled => _at * TimelineState.DevicePixelsPerSecond;
}
