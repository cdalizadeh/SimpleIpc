using System;
using PubSubIpc.Client;

namespace PubSubIpc.Server
{
    public class LocalSubscriber : ISubscriber
    {
        public IObservable<string> DataReceived {get;}

        public LocalSubscriber()
        {

        }

        public void Connect()
        {

        }

        public void Subscribe(string publisherId)
        {

        }

        public void Unsubscribe(string publisherId)
        {

        }
    }
}