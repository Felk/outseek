using System;
using Outseek.API;
using Outseek.API.Processors;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.ViewModels.TimelineObjects;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineSegmentViewModel : ViewModelBase
    {
        public TimelineState TimelineState { get; }
        
        [Reactive] public string? Text { get; set; }

        private ViewModelBase TimelineObject { get; }

        public TimelineSegmentViewModel() : this(new TimelineState(), new RandomSegmentsProcessor())
        {
            // the default constructor is only used by the designer
        }

        public TimelineSegmentViewModel(TimelineState timelineState, ITimelineProcessor processor)
        {
            TimelineState = timelineState;
            var context = new TimelineProcessContext(timelineState.Start, timelineState.End);
            TimelineObject = processor.Process(context, new TimelineObject.Nothing()) switch
            {
                TimelineObject.Nothing nothing => new NothingViewModel(),
                TimelineObject.Segments segments => new SegmentsViewModel(TimelineState, segments),
                _ => throw new ArgumentOutOfRangeException()
            };
            Text = processor.Name;
        }
    }
}
