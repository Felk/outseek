using Outseek.AvaloniaClient.SharedViewModels;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineProcessorParamsViewModel : ViewModelBase
    {
        public TimelineProcessorsState TimelineProcessorsState { get; }

        public TimelineProcessorParamsViewModel(TimelineProcessorsState timelineProcessorsState)
        {
            TimelineProcessorsState = timelineProcessorsState;
        }

        public TimelineProcessorParamsViewModel() : this(new TimelineProcessorsState())
        {
            // the default constructor is only used by the designer
        }
    }
}
