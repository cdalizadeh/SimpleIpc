using System;

namespace SimpleIpc.Shared
{
    /// <summary>
    /// Defines an interface for publishers to send messages and manage channels.
    /// </summary>
    public interface IPublisherClient : IDisposable
    {
        /// <summary>
        /// Sends a message to all channels currently being published to.
        /// </summary>
        /// <param name="message">The message to send</param>
        void Send(string message);

        /// <summary>
        /// Specifies a channel to begin publishing to.
        /// </summary>
        /// <param name="channelId">The channel ID</param>
        void Publish(string channelId);

        /// <summary>
        /// Specifies a channel to stop publishing to.
        /// </summary>
        /// <param name="channelId">The channel ID</param>
        void Unpublish(string channelId);
    }
}