using System;
using System.Reactive.Subjects;
using SimpleIpc.Shared;

namespace SimpleIpc.Server
{
    internal class LocalSubscriberClient : ISubscriberClient, ISubscriber
    {
        private Subject<string> _messageSubject = new Subject<string>();
        private readonly Subject<string> _subscribeReceived = new Subject<string>();
        private readonly Subject<string> _unsubscribeReceived = new Subject<string>();

        public IObservable<string> MessageReceived => _messageSubject;

        public IObserver<string> MessageObserver => _messageSubject;

        public IObservable<string> SubscribeReceived => _subscribeReceived;
        public IObservable<string> UnsubscribeReceived => _unsubscribeReceived;

        public LocalSubscriberClient()
        {

        }

        public void Subscribe(string channelId)
        {
            _subscribeReceived.OnNext(channelId);
        }

        public void Unsubscribe(string channelId)
        {
            _unsubscribeReceived.OnNext(channelId);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}