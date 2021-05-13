using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Outseek.API.Processors
{
    public class RandomSegmentsProcessor : ITimelineProcessor<TimelineObject.Nothing, TimelineObject.Segments>
    {
        public string Name => "random segments (testing)";

        public TimelineObject.Segments Process(TimelineProcessContext context, TimelineObject.Nothing input)
        {
            Random random = new();

            List<Segment> segments = new();
            const int numSegments = 3;

            List<int> cuts = Enumerable
                .Range(0, numSegments * 2)
                .Select(_ => random.Next((int) context.Minimum, (int) context.Maximum))
                .OrderBy(i => i)
                .ToList();

            for (int i = 0; i < cuts.Count; i += 2)
            {
                int from = cuts[i];
                int to = cuts[i + 1];
                segments.Add(new Segment(from, to));
            }

            return new TimelineObject.Segments(segments.ToImmutableList());
        }
    }
}
