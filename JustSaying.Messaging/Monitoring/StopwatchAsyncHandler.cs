using System.Diagnostics;
using System.Threading.Tasks;
using JustSaying.Messaging.MessageHandling;
using JustSaying.Models;

namespace JustSaying.Messaging.Monitoring
{
    public class StopwatchAsyncHandler<T> : IAsyncHandler<T> where T : Message
    {
        private readonly IAsyncHandler<T> _inner;
        private readonly IMeasureHandlerExecutionTime _monitoring;

        public StopwatchAsyncHandler(IAsyncHandler<T> inner, IMeasureHandlerExecutionTime monitoring)
        {
            _inner = inner;
            _monitoring = monitoring;
        }

        public async Task<bool> HandleAsync(T message)
        {
            var watch = Stopwatch.StartNew();
            var result = await _inner.HandleAsync(message);
            watch.Stop();
            _monitoring.HandlerExecutionTime(GetType().Name.ToLower(), message.GetType().Name.ToLower(),
                watch.Elapsed);
            return result;
        }
    }
}