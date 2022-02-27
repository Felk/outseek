using System;
using System.Collections.Generic;

namespace Outseek.API
{
    public record TimelineObject
    {
        internal TimelineObject()
        {
            // Private constructor to avoid external inheritance.
            // All derived types are defined within this class and sealed to mimic a sum type.
        }

        // public sealed record Multiple(IImmutableDictionary<string, TimelineObject> Objects) : TimelineObject;
        public sealed record Nothing : TimelineObject;
        public sealed record Error(Exception? Exception, string? ErrorText) : TimelineObject;
        public sealed record Segments(Func<IAsyncEnumerable<Segment>> SegmentList) : TimelineObject;
        public sealed record TimedText(Func<IAsyncEnumerable<TimedTextEntry>> Entries) : TimelineObject;
    }
}
