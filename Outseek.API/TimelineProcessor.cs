using System;

namespace Outseek.API
{
    public interface ITimelineProcessor
    {
        public string Name { get; }
        public Type InputType { get; }
        public Type OutputType { get; }
        public object GetDefaultParameters();

        public TimelineObject Process(TimelineProcessContext context, TimelineObject input, object parameters);
    }

    public interface ITimelineProcessor<in TIn, out TOut, in TParam> : ITimelineProcessor
        where TIn : TimelineObject
        where TOut : TimelineObject
        where TParam : class, new()
    {
        Type ITimelineProcessor.InputType => typeof(TIn);
        Type ITimelineProcessor.OutputType => typeof(TOut);
        object ITimelineProcessor.GetDefaultParameters() => new TParam();

        TimelineObject ITimelineProcessor.Process(
            TimelineProcessContext context, TimelineObject input, object parameters) =>
            Process(context, (TIn) input, (TParam) parameters);

        public TOut Process(TimelineProcessContext context, TIn input, TParam parameters);
    }
}
