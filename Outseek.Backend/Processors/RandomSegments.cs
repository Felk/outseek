using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Outseek.API;

namespace Outseek.Backend.Processors
{
    public class RandomSegments : ITimelineProcessor<TimelineObject.Nothing, TimelineObject.Segments>
    {
        public string Name => "random segments (testing)";

        public TimelineObject.Segments Process(TimelineProcessContext context, TimelineObject.Nothing input)
        {
            Random random = new();

            List<Segment> segments = new();
            const int numSegments = 20;

            List<int> cuts = Enumerable
                .Range(0, numSegments * 2)
                .Select(_ => random.Next((int) context.Minimum, (int) context.Maximum))
                .OrderBy(i => i)
                .Distinct()
                .ToList();

            async IAsyncEnumerable<Segment> GetSegments()
            {
                for (int i = 0; i + 1 < cuts.Count; i += 2)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(20));
                    int from = cuts[i];
                    int to = cuts[i + 1];
                    yield return new Segment(from, to);
                }
            }

            return new TimelineObject.Segments(GetSegments());
        }
    }
}
