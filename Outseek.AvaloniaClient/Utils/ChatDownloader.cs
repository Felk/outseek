using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public async IAsyncEnumerable<ChatMessage> GetChat(string url)
        {
            var chat = await Task.Run(() =>
            {
                using (Py.GIL()) return _chatDownloaderModule.ChatDownloader().get_chat(url);
            });

            IntPtr state = PythonEngine.AcquireLock();
            try
            {
                foreach (var message in chat)
                {
                    await Task.CompletedTask;
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
                        yield return msg;
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
        }
    }
}
