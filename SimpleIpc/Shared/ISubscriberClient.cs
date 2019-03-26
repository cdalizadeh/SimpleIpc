using System;

namespace SimpleIpc.Shared
{
    /// <summary>
    /// Defines an interface for subscribers to receive messages and manage channels.
    /// </summary>
    public interface ISubscriberClient : IDisposable
    {
        /// <summary>
        /// An Observable that emits messages recieved on channels subscribed to.
        /// </summary>
        /// <value>An IObservable of messages</value>
        IObservable<string> MessageReceived {get;}

        /// <summary>
        /// Specifies a channel to begin subscribing to.
        /// </summary>
        /// <param name="channelId">The channel ID</param>
        void Subscribe(string channelId);

        /// <summary>
        /// Specifies a channel to stop subscribing to.
        /// </summary>
        /// <param name="channelId">The channel ID</param>
        void Unsubscribe(string channelId);
    }
}