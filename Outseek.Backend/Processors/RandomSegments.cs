using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Outseek.API;

namespace Outseek.Backend.Processors;

public class RandomSegmentsProcessorParams : TimelineProcessorParams
{
    [DisplayName("Number of segments")] public int NumSegments { get; set; } = 20;
    [DisplayName("Delay between segments (ms)")] public int DelayBetweenSegmentsMs { get; set; } = 20;
}

public class RandomSegments
    : ITimelineProcessor<TimelineObject.Nothing, TimelineObject.Segments, RandomSegmentsProcessorParams>
{
    public string Name => "random segments (testing)";

    public TimelineObject.Segments Process(
        ITimelineProcessContext context, TimelineObject.Nothing input, RandomSegmentsProcessorParams parameters)
    {
        Random random = new();

        List<int> cuts = Enumerable
            .Range(0, parameters.NumSegments * 2)
            .Select(_ => random.Next((int) context.Minimum, (int) context.Maximum))
            .OrderBy(i => i)
            .Distinct()
            .ToList();

        async IAsyncEnumerable<Segment> GetSegments()
        {
            for (int i = 0; i + 1 < cuts.Count; i += 2)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(parameters.DelayBetweenSegmentsMs));
                int from = cuts[i];
                int to = cuts[i + 1];
                yield return new Segment(from, to);
            }
        }

        return new TimelineObject.Segments(GetSegments);
    }
}
