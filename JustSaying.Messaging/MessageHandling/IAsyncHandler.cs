using System.Threading.Tasks;

namespace JustSaying.Messaging.MessageHandling
{
    /// <summary>
    /// Async message handlers
    /// </summary>
    /// <typeparam name="T">Type of message to be handled</typeparam>
    public interface IAsyncHandler<in T> : IMayBeSyncOrAsyncHandler<T>
    {
        /// <summary>
        /// Handle a message of a given type
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Was handling successful?</returns>
        Task<bool> HandleAsync(T message);
    }
}