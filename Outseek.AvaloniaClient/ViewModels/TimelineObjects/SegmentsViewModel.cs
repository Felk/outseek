using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects
{
    public class SegmentsViewModel : TimelineObjectViewModelBase
    {
        private readonly TimelineObject.Segments _segments;
        public TimelineState TimelineState { get; }

        public ObservableCollection<SegmentViewModel> Segments { get; } = new();

        public SegmentsViewModel(TimelineState timelineState, TimelineObject.Segments segments)
        {
            TimelineState = timelineState;
            _segments = segments;
        }

        public SegmentsViewModel() : this(
            new TimelineState(), new TimelineObject.Segments(AsyncEnumerable.Empty<Segment>()))
        {
            // the default constructor is only used by the designer
        }

        public override async Task Refresh()
        {
            Segments.Clear();
            await foreach (Segment segment in _segments.SegmentList)
            {
                SegmentViewModel svm = new(TimelineState, segment.FromSeconds, segment.ToSeconds);
                Dispatcher.UIThread.Post(() => Segments.Add(svm));
            }
        }
    }

    public class SegmentViewModel : ViewModelBase
    {
        private double _from;
        private double _to;
        private TimelineState TimelineState { get; }

        public SegmentViewModel(TimelineState timelineState, double from, double to)
        {
            TimelineState = timelineState;
            _from = from;
            _to = to;
            TimelineState
                .WhenAnyValue(vm => vm.DevicePixelsPerSecond)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(StepScaled));
                    this.RaisePropertyChanged(nameof(FromScaled));
                    this.RaisePropertyChanged(nameof(ToScaled));
                });
            TimelineState
                .WhenAnyValue(vm => vm.Step)
                .Subscribe(_ => { this.RaisePropertyChanged(nameof(StepScaled)); });
        }
        
        public double From
        {
            get => _from;
            set
            {
                if (Equals(value, _from)) return;
                this.RaisePropertyChanging();
                this.RaisePropertyChanging(nameof(FromScaled));
                _from = value;
                this.RaisePropertyChanged(nameof(FromScaled));
                this.RaisePropertyChanged();
            }
        }

        public double To
        {
            get => _to;
            set
            {
                if (Equals(value, _to)) return;
                this.RaisePropertyChanging();
                this.RaisePropertyChanging(nameof(ToScaled));
                _to = value;
                this.RaisePropertyChanged(nameof(ToScaled));
                this.RaisePropertyChanged();
            }
        }

        public double FromScaled
        {
            get => From * TimelineState.DevicePixelsPerSecond;
            set => From = value / TimelineState.DevicePixelsPerSecond;
        }

        public double ToScaled
        {
            get => To * TimelineState.DevicePixelsPerSecond;
            set => To = value / TimelineState.DevicePixelsPerSecond;
        }

        public double StepScaled
        {
            get => TimelineState.Step * TimelineState.DevicePixelsPerSecond;
            set => TimelineState.Step = value / TimelineState.DevicePixelsPerSecond;
        }
    }
}
