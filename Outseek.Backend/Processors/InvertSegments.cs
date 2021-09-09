using System.Collections.Generic;
using Outseek.API;

namespace Outseek.Backend.Processors
{
    public class InvertSegmentsProcessorParams : TimelineProcessorParams
    {
    }

    public class InvertSegments : ITimelineProcessor<TimelineObject.Segments, TimelineObject.Segments, InvertSegmentsProcessorParams>
    {
        public string Name => "invert segments";

        public TimelineObject.Segments Process(ITimelineProcessContext context, TimelineObject.Segments input, InvertSegmentsProcessorParams parameters)
        {
            async IAsyncEnumerable<Segment> GetInvertedSegments()
            {
                Segment prevSegment = new(context.Minimum, context.Minimum);
                await foreach (Segment x in input.SegmentList)
                {
                    Segment invertedSegment = new(prevSegment.ToSeconds, x.FromSeconds);
                    if (invertedSegment.FromSeconds < invertedSegment.ToSeconds)
                        yield return invertedSegment;
                    prevSegment = x;
                }

                if (prevSegment.ToSeconds < context.Maximum)
                    yield return new Segment(prevSegment.ToSeconds, context.Maximum);
            }

            return new TimelineObject.Segments(GetInvertedSegments());
        }
    }
}
