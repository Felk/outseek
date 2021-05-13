using System.Collections.ObjectModel;
using Outseek.API.Processors;
using Outseek.AvaloniaClient.SharedViewModels;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }

        [Reactive] public ObservableCollection<TimelineSegmentViewModel> Segments { get; set; } = new();

        public TimelineViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }

        public TimelineViewModel(TimelineState timelineState)
        {
            TimelineState = timelineState;
            Segments.Add(new TimelineSegmentViewModel(TimelineState, new RandomSegmentsProcessor()));
            Segments.Add(new TimelineSegmentViewModel(TimelineState, new RandomSegmentsProcessor()));
            Segments.Add(new TimelineSegmentViewModel(TimelineState, new RandomSegmentsProcessor()));
        }
    }
}
