using System;

namespace Outseek.API
{
    public interface ITimelineProcessor
    {
        public string Name { get; }
        public Type InputType { get; }
        public Type OutputType { get; }

        public TimelineObject Process(TimelineProcessContext context, TimelineObject input);
    }

    public interface ITimelineProcessor<in TIn, out TOut> : ITimelineProcessor
        where TIn : TimelineObject
        where TOut : TimelineObject
    {
        Type ITimelineProcessor.InputType => typeof(TIn);
        Type ITimelineProcessor.OutputType => typeof(TOut);

        TimelineObject ITimelineProcessor.Process(TimelineProcessContext context, TimelineObject input) =>
            Process(context, (TIn) input);

        public TOut Process(TimelineProcessContext context, TIn input);
    }
}
