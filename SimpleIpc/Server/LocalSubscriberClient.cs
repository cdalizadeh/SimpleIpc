using System;
using System.Reactive.Subjects;
using SimpleIpc.Shared;

namespace SimpleIpc.Server
{
    /// <summary>
    /// A subscriber client running in the same process as the server. Publicy exposed through the ISubscriberClient interface.
    /// </summary>
    internal class LocalSubscriberClient : ISubscriberClient, ISubscriber
    {
        private Subject<string> _messageSubject = new Subject<string>();
        private readonly Subject<string> _subscribeReceived = new Subject<string>();
        private readonly Subject<string> _unsubscribeReceived = new Subject<string>();

        /// <inheritdoc /> 
        public IObservable<string> MessageReceived => _messageSubject;

        public IObserver<string> MessageObserver => _messageSubject;

        public IObservable<string> SubscribeReceived => _subscribeReceived;
        public IObservable<string> UnsubscribeReceived => _unsubscribeReceived;

        /// <summary>
        /// Creates a new <see cref="LocalSubscriberClient">.
        /// </summary>
        internal LocalSubscriberClient()
        {

        }

        /// <inheritdoc />
        public void Subscribe(string channelId)
        {
            _subscribeReceived.OnNext(channelId);
        }

        /// <inheritdoc />
        public void Unsubscribe(string channelId)
        {
            _unsubscribeReceived.OnNext(channelId);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _messageSubject.Dispose();
            _subscribeReceived.Dispose();
            _unsubscribeReceived.Dispose();
        }
    }
}