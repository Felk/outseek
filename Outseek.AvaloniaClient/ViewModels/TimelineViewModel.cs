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
        public WorkingAreaViewModel WorkingAreaViewModel { get; }

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
            WorkingAreaState workingAreaState = new();
            WorkingAreaViewModel = new WorkingAreaViewModel(timelineState, workingAreaState);

            InitializeTimeline = ReactiveCommand.Create((Func<Task>) (async () =>
            {
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments(), workingAreaState));
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new RandomSegments(), workingAreaState));
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new GetRandomChat(), workingAreaState));

                IncludedPython? py = await IncludedPython.Create();
                if (py == null) return;

                // currently passing a git install path using a fork because of: https://github.com/xenova/chat-downloader/issues/85#issuecomment-832182138
                dynamic? chatDownloaderModule = await py.GetModule(
                    importName: "chat_downloader", pypiName: "chat-downloader",
                    pipInstallName: "git+https://github.com/turbcool/chat-downloader.git#egg=chat-downloader");
                if (chatDownloaderModule == null) return;
                IChatDownloader chatDownloader = new ChatDownloader(chatDownloaderModule);
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new GetChat(chatDownloader), workingAreaState));
            }));
        }
    }
}
