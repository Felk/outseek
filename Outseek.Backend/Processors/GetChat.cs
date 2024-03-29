﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Outseek.API;

namespace Outseek.Backend.Processors;

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
    public IAsyncEnumerable<ChatMessage> GetChat(string url, string cacheStorageDir);
}

public class GetChatProcessorParams : TimelineProcessorParams
{
    public string Url { get; set; } = "https://www.twitch.tv/videos/1234567890";
}

public class GetChat : ITimelineProcessor<TimelineObject.Nothing, TimelineObject.TimedText, GetChatProcessorParams>
{
    private readonly IChatDownloader _chatDownloader;

    public GetChat(IChatDownloader chatDownloader)
    {
        _chatDownloader = chatDownloader;
    }

    public string Name => "Get chat from stream URL";

    public TimelineObject.TimedText Process(
        ITimelineProcessContext context, TimelineObject.Nothing input, GetChatProcessorParams parameters)
    {
        IAsyncEnumerable<TimedTextEntry> textEntries = _chatDownloader
            .GetChat(parameters.Url, context.GetStorageDirectory("downloaded_chats"))
            .Select(msg => new TimedTextEntry(msg.TimeInSeconds, msg.TimeInSeconds, msg.Message));
        var repeatableEnumerable = new RetainingAsyncEnumerable<TimedTextEntry>(textEntries);
        return new TimelineObject.TimedText(() => repeatableEnumerable);
    }
}
