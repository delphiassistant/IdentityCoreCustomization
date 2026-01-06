using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Services
{
    public interface IBackgroundEmailQueue
    {
        void QueueEmail(string email, string subject, string htmlMessage);
        Task<(string Email, string Subject, string HtmlMessage)> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundEmailQueue : IBackgroundEmailQueue
    {
        private readonly Channel<(string Email, string Subject, string HtmlMessage)> _queue;

        public BackgroundEmailQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<(string, string, string)>(options);
        }

        public void QueueEmail(string email, string subject, string htmlMessage)
        {
            ArgumentNullException.ThrowIfNull(email);
            ArgumentNullException.ThrowIfNull(subject);
            ArgumentNullException.ThrowIfNull(htmlMessage);

            _queue.Writer.TryWrite((email, subject, htmlMessage));
        }

        public async Task<(string Email, string Subject, string HtmlMessage)> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
