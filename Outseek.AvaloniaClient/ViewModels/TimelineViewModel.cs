using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.Backend.Processors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }
        public ZoomAdjusterViewModel ZoomAdjusterViewModel { get; }

        [Reactive] public ObservableCollection<TimelineObjectViewModel> TimelineObjects { get; set; } = new();
        [Reactive] public TimelineObjectViewModel SelectedTimelineObject { get; set; } = new();

        public ReactiveCommand<Unit, Task> InitializeTimeline { get; }

        public TimelineViewModel() : this(new TimelineState())
        {
            // the default constructor is only used by the designer
        }

        public TimelineViewModel(TimelineState timelineState)
        {
            TimelineState = timelineState;
            ZoomAdjusterViewModel = new ZoomAdjusterViewModel(timelineState);

            InitializeTimeline = ReactiveCommand.Create((Func<Task>) (async () =>
            {
                IncludedPython py = await IncludedPython.Create();

                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments()));
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments()));
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new GetRandomChat()));
            }));
        }
    }
}
