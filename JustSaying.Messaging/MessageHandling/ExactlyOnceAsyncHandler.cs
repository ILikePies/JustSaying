using System;
using System.Threading.Tasks;
using JustSaying.Models;

namespace JustSaying.Messaging.MessageHandling
{
    public class ExactlyOnceAsyncHandler<T> : IAsyncHandler<T> where T : Message
    {
        private readonly IAsyncHandler<T> _inner;
        private readonly IMessageLock _messageLock;
        private readonly int _timeOut;

        public ExactlyOnceAsyncHandler(IAsyncHandler<T> inner, IMessageLock messageLock, int timeOut)
        {
            _inner = inner;
            _messageLock = messageLock;
            _timeOut = timeOut;
        }

        public async Task<bool> HandleAsync(T message)
        {
            var lockKey = string.Format("{0}-{1}-{2}", _inner.GetType().FullName.ToLower(), typeof(T).Name.ToLower(), message.UniqueKey());
            bool canLock = _messageLock.TryAquire(lockKey, TimeSpan.FromSeconds(_timeOut));
            if (!canLock)
                return true;

            try
            {
                var successfullyHandled = await _inner.HandleAsync(message);
                if (successfullyHandled)
                {
                    _messageLock.TryAquire(lockKey);
                }
                return successfullyHandled;
            }
            catch
            {
                _messageLock.Release(lockKey);
                throw;
            }
        }
    }
}