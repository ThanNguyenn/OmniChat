using OmniChat.Application.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Channels
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue;

        public BackgroundTaskQueue()
        {
            _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>();
        }

        public async ValueTask QueueAsync(Func<CancellationToken, Task> workItem)
        {
            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
