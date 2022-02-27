using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Outseek.Backend.Processors;
using Python.Runtime;

namespace Outseek.AvaloniaClient.Utils;

public class ChatDownloader : IChatDownloader
{
    /// <summary>
    /// This must be a reference to the "chat_downloader" python module.
    /// </summary>
    private readonly dynamic _chatDownloaderModule;

    public ChatDownloader(dynamic chatDownloaderModule)
    {
        _chatDownloaderModule = chatDownloaderModule;
    }

    public IAsyncEnumerable<ChatMessage> GetChat(string url, string cacheStorageDir)
    {
        Channel<ChatMessage> channel = Channel.CreateUnbounded<ChatMessage>();

        Task _ = Task.Run((Func<Task>) (async () =>
        {
            string chatIdentifier = url.Split("://", count: 2)[^1];
            foreach (char invalid in Path.GetInvalidFileNameChars())
                chatIdentifier = chatIdentifier.Replace(invalid, '_');

            // maybe we already downloaded it earlier, check the expected file on disk
            string filepath = Path.Join(cacheStorageDir, chatIdentifier + ".jsonl.gz");
            if (File.Exists(filepath))
            {
                await using FileStream file = File.Open(filepath, FileMode.Open, FileAccess.Read);
                await using GZipStream gzipStream = new(file, CompressionMode.Decompress);
                StreamReader reader = new(gzipStream);
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    ChatMessage? message = JsonSerializer.Deserialize<ChatMessage>(line);
                    if (message != null)
                        await channel.Writer.WriteAsync(message);
                }

                return;
            }

            List<ChatMessage> messages = new();

            IntPtr state = PythonEngine.AcquireLock();

            try
            {
                dynamic chat = _chatDownloaderModule.ChatDownloader().get_chat(url);
                dynamic builtins = Py.Import("builtins");
                PyObject keyError = builtins.KeyError;

                // this loop is not async, so it needs to be offloaded onto a thread to not block the caller.
                foreach (var message in chat)
                {
                    var author = message["author"];
                    List<string> badges = new();
                    try
                    {
                        foreach (var badge in author["badges"])
                            badges.Add(badge["title"].As<string>());
                    }
                    catch (PythonException ex) when (ex.Type.Handle == keyError.Handle)
                    {
                    }

                    ChatMessage msg;
                    try
                    {
                        msg = new(
                            message["message_id"].As<string>(),
                            message["message"].As<string>(),
                            message["message_type"].As<string>(),
                            message["timestamp"].As<long>(),
                            message["time_in_seconds"].As<float>(), // TODO this field is missing for ongoing YT streams (and maybe in other scenarios too)
                            // author["id"].As<string>(), // see https://github.com/xenova/chat-downloader/pull/90
                            author["name"].As<string>(),
                            badges.ToImmutableList());
                    }
                    catch (PythonException ex)
                    {
                        // this sometimes fails for yet unknown reasons,
                        // but it's better to skip a few messages than to abort the entire download
                        await Console.Error.WriteLineAsync("Failed to read message data from python message dictionary, skipping object: " + message + "\n" + ex);
                        continue;
                    }

                    messages.Add(msg);
                    // awaiting yields to god knows whose code, so release the GIL for its duration
                    PythonEngine.ReleaseLock(state);
                    try
                    {
                        await channel.Writer.WriteAsync(msg);
                    }
                    finally
                    {
                        state = PythonEngine.AcquireLock();
                    }
                }
            }
            finally
            {
                PythonEngine.ReleaseLock(state);
            }

            // Successfully downloaded the entire chat. Cache it on disk so we don't have to do that again.
            var options = new JsonSerializerOptions {WriteIndented = false};
            await using (FileStream file = File.Open(filepath, FileMode.CreateNew, FileAccess.Write))
            await using (GZipStream gzipStream = new(file, CompressionMode.Compress))
            {
                StreamWriter writer = new(gzipStream);
                foreach (ChatMessage message in messages)
                    await writer.WriteLineAsync(JsonSerializer.Serialize(message, options));
                await writer.FlushAsync();
            }
        })).ContinueWith(task =>
        {
            if (task.IsFaulted) channel.Writer.Complete(task.Exception);
            else channel.Writer.Complete();
        });

        return channel.Reader.ReadAllAsync();
    }
}
