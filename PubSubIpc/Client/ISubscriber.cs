using System;

namespace PubSubIpc.Client
{
    interface ISubscriber
    {
        IObservable<string> DataReceived {get;}

        void Connect();
        void Subscribe(string publisherId);
        void Unsubscribe(string publisherId);
    }
}