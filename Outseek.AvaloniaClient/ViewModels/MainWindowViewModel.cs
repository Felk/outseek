using System;
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
    public class MainWindowViewModel : ViewModelBase
    {
        public TimelineViewModel TimelineViewModel { get; }
        public VideoplayerViewModel VideoplayerViewModel { get; }
        public TimelineProcessorParamsViewModel TimelineProcessorParamsViewModel { get; }
        public TimelineState TimelineState { get; }
        public MediaState MediaState { get; }
        public TimelineProcessorsState TimelineProcessorsState { get; }

        public ReactiveCommand<Unit, Task> Initialize { get; }

        public MainWindowViewModel()
        {
            TimelineState = new TimelineState();
            MediaState = new MediaState();
            TimelineProcessorsState = new TimelineProcessorsState();

            WorkingAreaState workingAreaState = new();
            var workingAreaViewModel = new WorkingAreaViewModel(TimelineState, workingAreaState);
            var workingAreaToolsViewModel = new WorkingAreaToolsViewModel(workingAreaViewModel, MediaState);

            TimelineViewModel = new TimelineViewModel(TimelineState, TimelineProcessorsState, workingAreaViewModel, workingAreaToolsViewModel);
            VideoplayerViewModel = new VideoplayerViewModel(TimelineState, MediaState);
            TimelineProcessorParamsViewModel = new TimelineProcessorParamsViewModel(TimelineProcessorsState);

            IObservable<TimelineProcessContext> context = TimelineState
                .WhenAnyValue(s => s.Start, s => s.End)
                .Select(tpl => new TimelineProcessContext(tpl.Item1, tpl.Item2));

            Initialize = ReactiveCommand.Create((Func<Task>)(async () =>
            {
                TimelineViewModel.TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new TimelineProcessorNode(new RandomSegments(), context), workingAreaState));
                TimelineViewModel.TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new TimelineProcessorNode(new RandomSegments(), context), workingAreaState));
                TimelineViewModel.TimelineObjects.Add(new TimelineObjectViewModel(TimelineState, new TimelineProcessorNode(new GetRandomChat(), context), workingAreaState));
                TimelineViewModel.TimelineObjects[0].Node.Children.Add(new TimelineProcessorNode(new InvertSegments(), context));
                TimelineViewModel.TimelineObjects[0].Node.Children.Add(new TimelineProcessorNode(new RandomSegments(), context));
                TimelineViewModel.TimelineObjects[0].Node.Children[0].Children.Add(new TimelineProcessorNode(new InvertSegments(), context));

                IncludedPython? py = await IncludedPython.Create();
                if (py == null) return;

                dynamic? chatDownloaderModule = await py.GetModule(importName: "chat_downloader", pypiName: "chat-downloader");
                if (chatDownloaderModule == null) return;
                IChatDownloader chatDownloader = new ChatDownloader(chatDownloaderModule);
                TimelineViewModel.TimelineObjects.Add(new TimelineObjectViewModel(
                    TimelineState, new TimelineProcessorNode(new GetChat(chatDownloader), context), workingAreaState));

                dynamic? otio = await py.GetModule("opentimelineio", "opentimelineio");
                workingAreaToolsViewModel.Otio = new OpenTimelineIO(otio);
            }));
        }
    }
}
