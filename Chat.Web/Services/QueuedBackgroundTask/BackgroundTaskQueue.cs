﻿using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Chat.Web.Services.QueuedBackgroundTask
{
    public interface IBackgroundTaskQueue
    {
        ValueTask EnqueueWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
        ValueTask<Func<CancellationToken, ValueTask>> DequeueWorkItemAsync( CancellationToken cancellationToken);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private  Channel<Func<CancellationToken, ValueTask>> _queue;

        

        public BackgroundTaskQueue(int capacity)
        {
            // Capacity should be set based on the expected application load and
            // number of concurrent threads accessing the queue.            
            // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
            // which completes only when space became available. This leads to backpressure,
            // in case too many publishers/calls start accumulating.

            //CreateBoundedChannel(capacity);
            CreateUnboundedChannel();
        }
        private void CreateUnboundedChannel()
        {
            var options = new UnboundedChannelOptions()
            {
                //FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false,
                AllowSynchronousContinuations = true,

            };
            _queue = Channel.CreateUnbounded<Func<CancellationToken, ValueTask>>();
        }
        private void CreateBoundedChannel(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        }

        public async ValueTask EnqueueWorkItemAsync(
            Func<CancellationToken, ValueTask> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _queue.Writer.WriteAsync(workItem);
        }

        public async ValueTask<Func<CancellationToken, ValueTask>> DequeueWorkItemAsync(
            CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }
}
