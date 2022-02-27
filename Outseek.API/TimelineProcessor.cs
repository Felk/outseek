using System;
using System.Linq;

namespace Outseek.API;

/// <summary>
/// Base object for timeline processor parameters that has default implementations for memberwise equality
/// and memberwise cloning.
/// If a derived class has members that require deep cloning or deep equality (e.g. a list),
/// it must override and properly implement <see cref="DeepCopy"/> and <see cref="Equals"/>
/// </summary>
public abstract class TimelineProcessorParams : IEquatable<TimelineProcessorParams>
{
    public TimelineProcessorParams DeepCopy() => (TimelineProcessorParams) MemberwiseClone();

    public bool Equals(TimelineProcessorParams? other)
    {
        if (other == null) return false;
        if (GetType() != other.GetType()) return false;
        return GetType().GetProperties()
            .All(propInfo => Equals(propInfo.GetValue(this), propInfo.GetValue(other)));
    }
}

public interface ITimelineProcessor
{
    public string Name { get; }
    public Type InputType { get; }
    public Type OutputType { get; }
    public TimelineProcessorParams GetDefaultParams();

    public TimelineObject Process(ITimelineProcessContext context, TimelineObject input, object parameters);
}

public interface ITimelineProcessor<in TIn, out TOut, in TParam> : ITimelineProcessor
    where TIn : TimelineObject
    where TOut : TimelineObject
    where TParam : TimelineProcessorParams, new()
{
    Type ITimelineProcessor.InputType => typeof(TIn);
    Type ITimelineProcessor.OutputType => typeof(TOut);
    TimelineProcessorParams ITimelineProcessor.GetDefaultParams() => new TParam();

    TimelineObject ITimelineProcessor.Process(
        ITimelineProcessContext context, TimelineObject input, object parameters) =>
        Process(context, (TIn) input, (TParam) parameters);

    public TOut Process(ITimelineProcessContext context, TIn input, TParam parameters);
}
