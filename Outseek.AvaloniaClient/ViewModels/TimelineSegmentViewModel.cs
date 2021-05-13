using System;
using Outseek.API;
using Outseek.API.Processors;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.ViewModels.TimelineObjects;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineSegmentViewModel : ViewModelBase
    {
        private readonly ITimelineProcessor _timelineProcessor;

        public TimelineState TimelineState { get; }

        [Reactive] public string? Text { get; set; }
        [Reactive] private ViewModelBase? TimelineObject { get; set; }

        public TimelineSegmentViewModel() : this(new TimelineState(), new RandomSegmentsProcessor())
        {
            // the default constructor is only used by the designer
        }

        private void RerunProcessor()
        {
            var context = new TimelineProcessContext(TimelineState.Start, TimelineState.End);
            TimelineObject = _timelineProcessor.Process(context, new TimelineObject.Nothing()) switch
            {
                TimelineObject.Nothing nothing => new NothingViewModel(),
                TimelineObject.Segments segments => new SegmentsViewModel(TimelineState, segments),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public TimelineSegmentViewModel(
            TimelineState timelineState, ITimelineProcessor processor)
        {
            TimelineState = timelineState;
            _timelineProcessor = processor;
            Text = _timelineProcessor.Name;
            RerunProcessor();
            timelineState
                .WhenAnyValue(t => t.Start, t => t.End)
                .Subscribe(_ => { RerunProcessor(); });
        }
    }
}
