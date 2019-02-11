using System;
using System.Reactive.Subjects;
using SimpleIpc.Shared;

namespace SimpleIpc.Server
{
    public class LocalPublisherClient : IPublisherClient, IPublisher
    {
        private readonly Subject<string> _messageReceived = new Subject<string>();
        private readonly Subject<string> _publishReceived = new Subject<string>();
        private readonly Subject<string> _unpublishReceived = new Subject<string>();
        
        public IObservable<string> MessageReceived => _messageReceived;
        public IObservable<string> PublishReceived => _publishReceived;
        public IObservable<string> UnpublishReceived => _unpublishReceived;

        public LocalPublisherClient()
        {

        }

        public void Send(string message)
        {
            _messageReceived.OnNext(message);
        }

        public void Publish(string channelId)
        {
            _publishReceived.OnNext(channelId);
        }

        public void Unpublish(string channelId)
        {
            _unpublishReceived.OnNext(channelId);
        }
    }
}