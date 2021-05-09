using System.Collections.ObjectModel;
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
            Segments.Add(new TimelineSegmentViewModel(TimelineState)
                {Text = "First Lane", From = 50, To = 200});
            Segments.Add(new TimelineSegmentViewModel(TimelineState)
                {Text = "Second Lane", From = 150, To = 230});
            Segments.Add(new TimelineSegmentViewModel(TimelineState)
                {Text = "Third Lane", From = 30, To = 120});
        }
    }
}
