using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Outseek.API;

namespace Outseek.Backend.Processors
{
    public record ChatMessage(
        string MessageId,
        string Message,
        string MessageType,
        long Timestamp,
        float TimeInSeconds,
        // string AuthorId,
        string AuthorName,
        IImmutableList<string> Badges);

    public interface IChatDownloader
    {
        public IAsyncEnumerable<ChatMessage> GetChat(string url);
    }

    public struct GetChatParams
    {
        public string Url { get; set; }
    }

    public class GetChat : ITimelineProcessor<TimelineObject.Nothing, TimelineObject.TimedText, GetChatParams>
    {
        private readonly IChatDownloader _chatDownloader;

        public GetChat(IChatDownloader chatDownloader)
        {
            _chatDownloader = chatDownloader;
        }

        public string Name => "Get chat from stream URL";

        public GetChatParams GetDefaultParams() => new()
        {
            Url = "https://www.twitch.tv/videos/1234567890"
        };

        public TimelineObject.TimedText Process(
            TimelineProcessContext context, TimelineObject.Nothing input, GetChatParams parameters)
        {
            IAsyncEnumerable<TimedTextEntry> textEntries = _chatDownloader
                .GetChat(parameters.Url)
                .Select(msg => new TimedTextEntry(msg.TimeInSeconds, msg.TimeInSeconds, msg.Message));
            return new TimelineObject.TimedText(textEntries);
        }
    }
}
