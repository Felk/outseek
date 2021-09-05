using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using DynamicData;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.ViewModels.TimelineObjects;
using Outseek.Backend.Processors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public sealed class TimelineObjectViewModel : ViewModelBase, IDisposable
    {
        public readonly TimelineProcessorNode Node;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TimelineState TimelineState { get; }

        [Reactive] public string? Text { get; set; }
        [Reactive] public TimelineObjectViewModelBase? TimelineObject { get; set; }

        public ReactiveCommand<Unit, Unit> CopySegments { get; }

        public TimelineObjectViewModel() : this(
            new TimelineState(), new TimelineProcessorNode(new RandomSegments(), Observable.Empty<ITimelineProcessContext>()), new WorkingAreaState())
        {
            // the default constructor is only used by the designer
        }

        public TimelineObjectViewModel(
            TimelineState timelineState,
            TimelineProcessorNode node,
            WorkingAreaState workingAreaState)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            TimelineState = timelineState;
            Node = node;
            Text = Node.Processor.Name;

            Node.WhenAnyValue(n => n.Output)
                .Subscribe(async timelineObject =>
                {
                    TimelineObjectViewModelBase timelineObjectViewModelBase = timelineObject switch
                    {
                        TimelineObject.Nothing nothing => new NothingViewModel(),
                        TimelineObject.Error error => new ErrorViewModel { ErrorText = error.ErrorText },
                        TimelineObject.Segments segments => new SegmentsViewModel(TimelineState, segments),
                        TimelineObject.TimedText timedText => new TimedTextViewModel(TimelineState, timedText),
                        _ => throw new Exception($"unknown timeline object '{timelineObject}'")
                    };
                    TimelineObject = timelineObjectViewModelBase;
                    try
                    {
                        await timelineObjectViewModelBase.Refresh(_cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        TimelineObject = new ErrorViewModel { ErrorText = ex.Message };
                    }
                });

            ObservableRange Clone(ObservableRange r) => new(timelineState, r.Range);
            CopySegments = ReactiveCommand.Create(
                () => workingAreaState.Segments.AddRange(((SegmentsViewModel)TimelineObject!).SelectedSegments.Select(Clone)),
                this.WhenAnyValue(vm => vm.TimelineObject).Select(to => to is SegmentsViewModel));
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
