using System;

namespace Outseek.API
{

    public interface ITimelineProcessor
    {
        public string Name { get; }
        public Type InputType { get; }
        public Type OutputType { get; }
        public ValueType GetDefaultParams();

        public TimelineObject Process(TimelineProcessContext context, TimelineObject input, object parameters);
    }

    public interface ITimelineProcessor<in TIn, out TOut, TParam> : ITimelineProcessor
        where TIn : TimelineObject
        where TOut : TimelineObject
        where TParam : struct
    {
        Type ITimelineProcessor.InputType => typeof(TIn);
        Type ITimelineProcessor.OutputType => typeof(TOut);
        ValueType ITimelineProcessor.GetDefaultParams() => GetDefaultParams();
        new TParam GetDefaultParams();

        TimelineObject ITimelineProcessor.Process(
            TimelineProcessContext context, TimelineObject input, object parameters) =>
            Process(context, (TIn) input, (TParam) parameters);

        public TOut Process(TimelineProcessContext context, TIn input, TParam parameters);
    }
}
