using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.Utils;
using Outseek.Backend.Processors;
using ReactiveUI;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }
        public TimelineProcessorsState TimelineProcessorsState { get; }
        public ZoomAdjusterViewModel ZoomAdjusterViewModel { get; }
        public WorkingAreaViewModel WorkingAreaViewModel { get; }

        public readonly ObservableCollection<TimelineObjectViewModel> TimelineObjects = new();

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

            IObservable<TimelineProcessContext> context = TimelineState
                .WhenAnyValue(s => s.Start, s => s.End)
                .Select(tpl => new TimelineProcessContext(tpl.Item1, tpl.Item2));

            InitializeTimeline = ReactiveCommand.Create((Func<Task>)(async () =>
            {
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new TimelineProcessorNode(new RandomSegments(), context), workingAreaState));
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new TimelineProcessorNode(new RandomSegments(), context), workingAreaState));
                TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new TimelineProcessorNode(new GetRandomChat(), context), workingAreaState));

                IncludedPython? py = await IncludedPython.Create();
                if (py == null) return;

                dynamic? chatDownloaderModule = await py.GetModule(importName: "chat_downloader", pypiName: "chat-downloader");
                if (chatDownloaderModule == null) return;
                IChatDownloader chatDownloader = new ChatDownloader(chatDownloaderModule);
                TimelineObjects.Add(new TimelineObjectViewModel(
                    TimelineState, new TimelineProcessorNode(new GetChat(chatDownloader), context), workingAreaState));
            }));
        }
    }
}
