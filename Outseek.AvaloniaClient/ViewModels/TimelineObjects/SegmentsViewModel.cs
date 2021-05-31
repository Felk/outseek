using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI;
using Range = Outseek.AvaloniaClient.Utils.Range;

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
            TimelineState.WhenAnyValue(t => t.DevicePixelsPerSecond)
                .Subscribe(_ =>
                {
                    foreach (SegmentViewModel segmentViewModel in Segments)
                    {
                        segmentViewModel.RaisePropertyChanged(nameof(SegmentViewModel.StepScaled));
                        segmentViewModel.RaisePropertyChanged(nameof(SegmentViewModel.RangeScaled));
                    }
                });
            TimelineState.WhenAnyValue(t => t.Step)
                .Subscribe(_ =>
                {
                    foreach (SegmentViewModel segmentViewModel in Segments)
                    {
                        segmentViewModel.RaisePropertyChanged(nameof(SegmentViewModel.StepScaled));
                    }
                });
        }

        public SegmentsViewModel() : this(
            new TimelineState(), new TimelineObject.Segments(AsyncEnumerable.Empty<Segment>()))
        {
            // the default constructor is only used by the designer
        }

        public override async Task Refresh(CancellationToken cancellationToken)
        {
            Segments.Clear();
            await foreach (Segment segment in _segments.SegmentList.WithCancellation(cancellationToken))
            {
                SegmentViewModel svm = new(TimelineState, new Range(segment.FromSeconds, segment.ToSeconds));
                Dispatcher.UIThread.Post(() => Segments.Add(svm));
            }
        }
    }

    public class SegmentViewModel : ViewModelBase
    {
        private Range _range;
        private TimelineState TimelineState { get; }

        public SegmentViewModel(TimelineState timelineState, Range range)
        {
            TimelineState = timelineState;
            _range = range;
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
}
