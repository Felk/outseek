using Outseek.AvaloniaClient.SharedViewModels;

namespace Outseek.AvaloniaClient.ViewModels;

public class WorkingAreaViewModel : ViewModelBase
{
    public TimelineState TimelineState { get; }

    public WorkingAreaState WorkingAreaState { get; }

    public WorkingAreaViewModel(TimelineState timelineState, WorkingAreaState workingAreaState)
    {
        TimelineState = timelineState;
        WorkingAreaState = workingAreaState;
    }

    public WorkingAreaViewModel() : this(new TimelineState(), new WorkingAreaState())
    {
        // the default constructor is only used by the designer
    }
}
