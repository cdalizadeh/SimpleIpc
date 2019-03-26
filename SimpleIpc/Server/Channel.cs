using System;
using System.Collections.Generic;

namespace SimpleIpc.Server
{
    /// <summary>
    /// An internal server representation of a channel.
    /// </summary>
    internal class Channel
    {
        private List<ISubscriber> _subscribers = new List<ISubscriber>();
        private List<IPublisher> _publishers = new List<IPublisher>();

        private Dictionary<Tuple<IPublisher, ISubscriber>, IDisposable> _subscriptions = new Dictionary<Tuple<IPublisher, ISubscriber>, IDisposable>();

        public bool IsEmpty => _subscribers.Count == 0 && _publishers.Count == 0;

        public Channel()
        {

        }

        public void AddPublisher(IPublisher publisher)
        {
            if (_publishers.Contains(publisher)) throw new InvalidOperationException("Publisher is already publishing to channel");

            _publishers.Add(publisher);
            foreach(var subscriber in _subscribers)
            {
                CreateSubscription(publisher, subscriber);
            }
        }

        public void RemovePublisher(IPublisher publisher)
        {
            _publishers.Remove(publisher);
            foreach(var subscriber in _subscribers)
            {
                RemoveSubscription(publisher, subscriber);
            }
        }

        public void AddSubscriber(ISubscriber subscriber)
        {
            if (_subscribers.Contains(subscriber)) throw new InvalidOperationException("Subscriber is already subscribing to channel");

            _subscribers.Add(subscriber);
            foreach(var publisher in _publishers)
            {
                CreateSubscription(publisher, subscriber);
            }
        }

        public void RemoveSubscriber(ISubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
            foreach(var publisher in _publishers)
            {
                RemoveSubscription(publisher, subscriber);
            }
        }

        private void CreateSubscription(IPublisher publisher, ISubscriber subscriber)
        {
            var subscription = publisher.MessageReceived.Subscribe(subscriber.MessageObserver);
            _subscriptions[Tuple.Create(publisher, subscriber)] = subscription;
        }

        private void RemoveSubscription(IPublisher publisher, ISubscriber subscriber)
        {
            _subscriptions[Tuple.Create(publisher, subscriber)].Dispose();
        }
    }
}