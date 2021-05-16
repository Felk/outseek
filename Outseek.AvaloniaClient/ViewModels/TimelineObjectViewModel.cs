﻿using System;
using System.Threading.Tasks;
using Outseek.API;
using Outseek.AvaloniaClient.SharedViewModels;
using Outseek.AvaloniaClient.ViewModels.TimelineObjects;
using Outseek.Backend.Processors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Outseek.AvaloniaClient.ViewModels
{
    public class TimelineObjectViewModel : ViewModelBase
    {
        private readonly ITimelineProcessor _timelineProcessor;

        public TimelineState TimelineState { get; }

        [Reactive] public string? Text { get; set; }
        [Reactive] private TimelineObjectViewModelBase? TimelineObject { get; set; }

        public TimelineObjectViewModel() : this(new TimelineState(), new RandomSegments())
        {
            // the default constructor is only used by the designer
        }

        private async Task RerunProcessor()
        {
            var context = new TimelineProcessContext(TimelineState.Start, TimelineState.End);
            try
            {
                TimelineObject timelineObject = _timelineProcessor.Process(context, new TimelineObject.Nothing());
                TimelineObjectViewModelBase timelineObjectViewModelBase = timelineObject switch
                {
                    TimelineObject.Nothing nothing => new NothingViewModel(),
                    TimelineObject.Segments segments => new SegmentsViewModel(TimelineState, segments),
                    _ => throw new ArgumentOutOfRangeException()
                };
                TimelineObject = timelineObjectViewModelBase;
                await timelineObjectViewModelBase.Refresh();
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
            TimelineState = timelineState;
            _timelineProcessor = processor;
            Text = _timelineProcessor.Name;
            var _ = RerunProcessor();
            timelineState
                .WhenAnyValue(t => t.Start, t => t.End)
                .Subscribe(async _ => { await RerunProcessor(); });
        }
    }
}
