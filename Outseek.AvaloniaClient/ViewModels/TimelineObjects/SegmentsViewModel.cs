using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.Utils;

namespace Outseek.AvaloniaClient.ViewModels.TimelineObjects
{
    public class SegmentsViewModel : TimelineObjectViewModelBase
    {
        private readonly TimelineObject.Segments _segments;
        public TimelineState TimelineState { get; }

        public ObservableCollection<ObservableRange> Segments { get; } = new();
        public ObservableCollection<ObservableRange> SelectedSegments { get; } = new();

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

        public override async Task Refresh(CancellationToken cancellationToken)
        {
            Segments.Clear();
            await foreach (Segment segment in _segments.SegmentList.WithCancellation(cancellationToken))
            {
                Dispatcher.UIThread.Post(() => Segments.Add(new ObservableRange(TimelineState, new Range(segment.FromSeconds, segment.ToSeconds))));
            }
        }
    }
}
