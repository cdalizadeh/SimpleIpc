using System;
using PubSubIpc.Shared;

namespace PubSubIpc.Server
{
    public class LocalSubscriberClient : Subscriber, ISubscriberClient, ISubscriber
    {
        public IObservable<string> DataReceived {get;}

        public LocalSubscriberClient()
        {
            DataReceived = _dataReceived;
        }
    }
}