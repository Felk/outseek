using System.Collections.ObjectModel;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.Backend.Processors;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }
        public ZoomAdjusterViewModel ZoomAdjusterViewModel { get; }

        [Reactive] public ObservableCollection<TimelineObjectViewModel> TimelineObjects { get; set; } = new();

        public TimelineViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }

        public TimelineViewModel(TimelineState timelineState)
        {
            TimelineState = timelineState;
            ZoomAdjusterViewModel = new ZoomAdjusterViewModel(timelineState);
            TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments()));
            TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments()));
            TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments()));
        }
    }
}
