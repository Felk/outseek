using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Outseek.API;

namespace Outseek.Backend.Processors;

public class GetRandomChatProcessorParams : TimelineProcessorParams
{
    [DisplayName("Messages per second")] public double MessagesPerSecond { get; set; } = 2;
}

public class GetRandomChat
    : ITimelineProcessor<TimelineObject.Nothing, TimelineObject.TimedText, GetRandomChatProcessorParams>
{
    public string Name => "random chat (testing)";

    public TimelineObject.TimedText Process(
        ITimelineProcessContext context, TimelineObject.Nothing input, GetRandomChatProcessorParams parameters)
    {
        double duration = context.Maximum - context.Minimum;
        int numMessages = (int) (duration * parameters.MessagesPerSecond);

        Random random = new();

        async IAsyncEnumerable<TimedTextEntry> GetEntries()
        {
            int i = 0;
            foreach (double second in Enumerable.Range(0, numMessages)
                         .Select(_ => random.NextDouble() * duration + context.Minimum)
                         .OrderBy(i => i))
            {
                if (i++ % 50 == 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                yield return new TimedTextEntry(second, second, $"dummy message text at second {second}");
            }
        }

        return new TimelineObject.TimedText(GetEntries);
    }
}
