using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly ITimelineProcessor _timelineProcessor;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TimelineState TimelineState { get; }

        [Reactive] public string? Text { get; set; }
        [Reactive] public TimelineProcessorParams ProcessorParamsObject { get; set; }
        [Reactive] public TimelineObjectViewModelBase? TimelineObject { get; set; }

        public ReactiveCommand<Unit, Unit> CopySegments { get; }
        
        public TimelineObjectViewModel() : this(new TimelineState(), new RandomSegments(), new WorkingAreaState())
        {
            // the default constructor is only used by the designer
        }

        public async Task RerunProcessor(CancellationToken cancellationToken)
        {
            var context = new TimelineProcessContext(TimelineState.Start, TimelineState.End);
            try
            {
                TimelineObject timelineObject = _timelineProcessor.Process(
                    context, new TimelineObject.Nothing(), ProcessorParamsObject);
                TimelineObjectViewModelBase timelineObjectViewModelBase = timelineObject switch
                {
                    TimelineObject.Nothing nothing => new NothingViewModel(),
                    TimelineObject.Segments segments => new SegmentsViewModel(TimelineState, segments),
                    TimelineObject.TimedText timedText => new TimedTextViewModel(TimelineState, timedText),
                    _ => throw new Exception($"unknown timeline object '{timelineObject}'")
                };
                TimelineObject = timelineObjectViewModelBase;
                await timelineObjectViewModelBase.Refresh(cancellationToken);
            }
            catch (Exception ex)
            {
                TimelineObject = new ErrorViewModel {Exception = ex, Error = ex.Message};
            }
        }

        public TimelineObjectViewModel(
            TimelineState timelineState,
            ITimelineProcessor processor,
            WorkingAreaState workingAreaState)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            TimelineState = timelineState;
            _timelineProcessor = processor;
            Text = _timelineProcessor.Name;
            this.WhenAnyValue(t => t.ProcessorParamsObject)
                .Subscribe(async paramsObj =>
                {
                    if (paramsObj != null) await RerunProcessor(CancellationToken.None);
                });
            ProcessorParamsObject = _timelineProcessor.GetDefaultParams();

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
