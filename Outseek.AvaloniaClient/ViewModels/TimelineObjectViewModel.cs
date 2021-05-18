using System;
using System.Threading;
using System.Threading.Tasks;
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
        [Reactive] public object ParameterObject { get; set; }
        [Reactive] private TimelineObjectViewModelBase? TimelineObject { get; set; }

        public TimelineObjectViewModel() : this(new TimelineState(), new RandomSegments())
        {
            // the default constructor is only used by the designer
        }

        private async Task RerunProcessor(CancellationToken cancellationToken)
        {
            var context = new TimelineProcessContext(TimelineState.Start, TimelineState.End);
            try
            {
                TimelineObject timelineObject = _timelineProcessor.Process(
                    context, new TimelineObject.Nothing(), ParameterObject);
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
                Console.Error.WriteLine(ex);
                TimelineObject = new ErrorViewModel {Error = ex.Message};
            }
        }

        public TimelineObjectViewModel(
            TimelineState timelineState, ITimelineProcessor processor)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            TimelineState = timelineState;
            _timelineProcessor = processor;
            Text = _timelineProcessor.Name;
            ParameterObject = _timelineProcessor.GetDefaultParameters();
            var _ = RerunProcessor(_cancellationTokenSource.Token);
            timelineState
                .WhenAnyValue(t => t.Start, t => t.End)
                .Subscribe(async _ => { await RerunProcessor(_cancellationTokenSource.Token); });
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
