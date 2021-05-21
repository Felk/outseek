using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.Utils;
using Outseek.Backend.Processors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }
        public TimelineProcessorsState TimelineProcessorsState { get; }
        public ZoomAdjusterViewModel ZoomAdjusterViewModel { get; }

        [Reactive] public ObservableCollection<TimelineObjectViewModel> TimelineObjects { get; set; } = new();

        public ReactiveCommand<Unit, Task> InitializeTimeline { get; }

        public TimelineViewModel() : this(new TimelineState(), new TimelineProcessorsState())
        {
            // the default constructor is only used by the designer
        }

        public TimelineViewModel(TimelineState timelineState, TimelineProcessorsState timelineProcessorsState)
        {
            TimelineState = timelineState;
            TimelineProcessorsState = timelineProcessorsState;
            ZoomAdjusterViewModel = new ZoomAdjusterViewModel(timelineState);

            InitializeTimeline = ReactiveCommand.Create((Func<Task>) (async () =>
            {
                IncludedPython py = await IncludedPython.Create();

                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments()));
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments()));
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new GetRandomChat()));

                dynamic chatDownloaderModule = await py.GetModule("chat_downloader", "chat-downloader");
                IChatDownloader chatDownloader = new ChatDownloader(chatDownloaderModule);
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new GetChat(chatDownloader)));
            }));
        }
    }
}
