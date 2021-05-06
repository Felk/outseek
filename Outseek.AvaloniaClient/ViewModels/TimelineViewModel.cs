using System;
using System.Collections.ObjectModel;
using Outseek.AvaloniaClient.Converters;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }

        [Reactive] public ObservableCollection<TimelineSegmentViewModel> Segments { get; set; } = new();
        [Reactive] public double MinSliderValue { get; set; }

        public TimelineViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }

        public TimelineViewModel(TimelineState timelineState)
        {
            TimelineState = timelineState;
            Segments.Add(new TimelineSegmentViewModel(TimelineState)
                {Text = "First Lane", From = 50, To = 200});
            Segments.Add(new TimelineSegmentViewModel(TimelineState)
                {Text = "Second Lane", From = 150, To = 230});
            Segments.Add(new TimelineSegmentViewModel(TimelineState)
                {Text = "Third Lane", From = 30, To = 120});

            TimelineState
                .WhenAnyValue(t => t.Start, t => t.End)
                .Subscribe(((double start, double end) tuple) =>
                {
                    double duration = tuple.end - tuple.start;
                    const double targetDevicePixels = 100;
                    double targetDevicePixelsPerSecond = targetDevicePixels / duration;
                    MinSliderValue = Math.Pow(targetDevicePixelsPerSecond, 1d / ZoomPowConverter.Exponent);
                });
        }
    }
}
