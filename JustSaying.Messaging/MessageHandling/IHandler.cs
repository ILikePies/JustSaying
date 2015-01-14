namespace JustSaying.Messaging.MessageHandling
{
    /// <summary>
    /// Marker interface in order to avoid massive restructuring of fluent syntax
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMayBeSyncOrAsyncHandler<in T>
    {
    }

    /// <summary>
    /// Message handlers
    /// </summary>
    /// <typeparam name="T">Type of message to be handled</typeparam>
    public interface IHandler<in T> : IMayBeSyncOrAsyncHandler<T>
    {
        /// <summary>
        /// Handle a message of a given type
        /// </summary>
        /// <param name="message">Message to handle</param>
        /// <returns>Was handling successful?</returns>
        bool Handle(T message);
    }
}