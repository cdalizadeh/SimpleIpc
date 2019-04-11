using System;
using System.Reactive.Subjects;
using SimpleIpc.Shared;

namespace SimpleIpc.Server
{
    /// <summary>
    /// A publisher client running in the same process as the server. Publicly exposed through the IPublisherClient interface.
    /// </summary>
    internal class LocalPublisherClient : IPublisherClient, IPublisher
    {
        private readonly Subject<string> _messageReceived = new Subject<string>();
        private readonly Subject<string> _publishReceived = new Subject<string>();
        private readonly Subject<string> _unpublishReceived = new Subject<string>();
        
        public IObservable<string> MessageReceived => _messageReceived;
        public IObservable<string> PublishReceived => _publishReceived;
        public IObservable<string> UnpublishReceived => _unpublishReceived;

        /// <summary>
        /// Creates a new <see cref="LocalPublisherClient">.
        /// </summary>
        internal LocalPublisherClient()
        {

        }

        /// <inheritdoc />
        public void Send(string message)
        {
            _messageReceived.OnNext(message);
        }

        /// <inheritdoc />
        public void Publish(string channelId)
        {
            _publishReceived.OnNext(channelId);
        }

        /// <inheritdoc />
        public void Unpublish(string channelId)
        {
            _unpublishReceived.OnNext(channelId);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _messageReceived.Dispose();
            _publishReceived.Dispose();
            _unpublishReceived.Dispose();
        }
    }
}