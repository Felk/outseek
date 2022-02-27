using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Outseek.API;

namespace Outseek.Backend.Processors;

public class AnalyzeChatParams : TimelineProcessorParams
{
    [DisplayName("Window (seconds)")] public double Window { get; set; } = 5;

    [DisplayName("Threshold (texts per second)")]
    public double Threshold { get; set; } = 1;

    [DisplayName("Offset (seconds)")] public double Offset { get; set; } = 0;
}

public class AnalyzeTimedText : ITimelineProcessor<TimelineObject.TimedText, TimelineObject.Segments, AnalyzeChatParams>
{
    public string Name => "analyze timed text";

    public TimelineObject.Segments Process(ITimelineProcessContext context, TimelineObject.TimedText input, AnalyzeChatParams parameters)
    {
        async IAsyncEnumerable<Segment> GetSegments()
        {
            // TODO context.Minimum aware?
            IAsyncEnumerable<(int, double)> tpses = input.Entries()
                .GroupBy(t => (int)(t.FromSeconds / parameters.Window))
                .SelectAwait(async grp => (grp.Key, await grp.CountAsync() / parameters.Window));

            int prevPos = 0;
            double? windowStart = null;
            await foreach ((int pos, double tps) in tpses)
            {
                if (windowStart != null && (pos > prevPos + 1 || tps < parameters.Threshold))
                {
                    yield return new Segment(windowStart.Value * parameters.Window + parameters.Offset, (prevPos+1) * parameters.Window + parameters.Offset);
                    windowStart = null;
                }
                else if (tps >= parameters.Threshold)
                {
                    windowStart ??= pos;
                }

                prevPos = pos;
            }

            if (windowStart != null)
            {
                yield return new Segment(windowStart.Value * parameters.Window + parameters.Offset, prevPos * parameters.Window + parameters.Offset);
            }
        }

        return new TimelineObject.Segments(GetSegments);
    }
}
