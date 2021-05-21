using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Outseek.API;

namespace Outseek.Backend.Processors
{
    public struct GetRandomChatParams
    {
    }

    public class GetRandomChat
        : ITimelineProcessor<TimelineObject.Nothing, TimelineObject.TimedText, GetRandomChatParams>
    {
        public string Name => "random chat (testing)";
        public GetRandomChatParams GetDefaultParams() => new();

        public TimelineObject.TimedText Process(
            TimelineProcessContext context, TimelineObject.Nothing input, GetRandomChatParams parameters)
        {
            double duration = context.Maximum - context.Minimum;
            const int messagesPerSecond = 2;
            int numMessages = (int) duration * messagesPerSecond;

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

            return new TimelineObject.TimedText(GetEntries());
        }
    }
}
