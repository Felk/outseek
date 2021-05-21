using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Channels;
using System.Threading.Tasks;
using Outseek.Backend.Processors;
using Python.Runtime;

namespace Outseek.AvaloniaClient.Utils
{
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

        public IAsyncEnumerable<ChatMessage> GetChat(string url)
        {
            Channel<ChatMessage> channel = Channel.CreateUnbounded<ChatMessage>();

            Task _ = Task.Run((Func<Task>) (async () =>
            {
                IntPtr state = PythonEngine.AcquireLock();

                try
                {
                    dynamic chat = _chatDownloaderModule.ChatDownloader().get_chat(url);
                
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
                        catch (PythonException ex) when (ex.PyType == Exceptions.KeyError)
                        {
                        }

                        ChatMessage msg = new(
                            message["message_id"].As<string>(),
                            message["message"].As<string>(),
                            message["message_type"].As<string>(),
                            message["timestamp"].As<long>(),
                            message["time_in_seconds"].As<int>(),
                            // author["id"].As<string>(), // see https://github.com/xenova/chat-downloader/pull/90
                            author["name"].As<string>(),
                            badges.ToImmutableList());
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
                catch (Exception ex)
                {
                    channel.Writer.Complete(ex);
                }
                finally
                {
                    PythonEngine.ReleaseLock(state);
                }
            }));

            return channel.Reader.ReadAllAsync();
        }
    }
}
