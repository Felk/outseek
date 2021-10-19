using System.Collections.ObjectModel;
using Outseek.AvaloniaClient.SharedViewModels;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }
        public TimelineProcessorsState TimelineProcessorsState { get; }
        public ZoomAdjusterViewModel ZoomAdjusterViewModel { get; }
        public WorkingAreaViewModel WorkingAreaViewModel { get; }
        public WorkingAreaToolsViewModel WorkingAreaToolsViewModel { get; }

        public ObservableCollection<TimelineObjectViewModel> TimelineObjects { get; } = new();

        public TimelineViewModel() : this(new TimelineState(), new TimelineProcessorsState(), new WorkingAreaViewModel(), new WorkingAreaToolsViewModel())
        {
            // the default constructor is only used by the designer
        }

        public TimelineViewModel(
            TimelineState timelineState,
            TimelineProcessorsState timelineProcessorsState,
            WorkingAreaViewModel workingAreaViewModel,
            WorkingAreaToolsViewModel workingAreaToolsViewModel)
        {
            TimelineState = timelineState;
            TimelineProcessorsState = timelineProcessorsState;
            ZoomAdjusterViewModel = new ZoomAdjusterViewModel(timelineState);
            WorkingAreaViewModel = workingAreaViewModel;
            WorkingAreaToolsViewModel = workingAreaToolsViewModel;
        }
    }
}
