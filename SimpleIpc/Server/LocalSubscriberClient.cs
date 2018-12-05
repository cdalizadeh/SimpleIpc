using System;
using SimpleIpc.Shared;

namespace SimpleIpc.Server
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