using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Services
{
    public interface IBackgroundSmsQueue
    {
        void QueueSms(string smsText, string receipentPhone);
        void QueueSms(string smsText, List<string> receipentPhones);
        Task<(string SmsText, List<string> Phones)> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundSmsQueue : IBackgroundSmsQueue
    {
        private readonly Channel<(string SmsText, List<string> Phones)> _queue;

        public BackgroundSmsQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<(string, List<string>)>(options);
        }

        public void QueueSms(string smsText, string receipentPhone)
        {
            ArgumentNullException.ThrowIfNull(smsText);
            ArgumentNullException.ThrowIfNull(receipentPhone);

            _queue.Writer.TryWrite((smsText, new List<string> { receipentPhone }));
        }

        public void QueueSms(string smsText, List<string> receipentPhones)
        {
            ArgumentNullException.ThrowIfNull(smsText);
            ArgumentNullException.ThrowIfNull(receipentPhones);

            _queue.Writer.TryWrite((smsText, receipentPhones));
        }

        public async Task<(string SmsText, List<string> Phones)> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
