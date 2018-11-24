using System;

namespace PubSubIpc
{
    interface ISubscriber
    {
        IObservable<string> DataReceived {get;}

        void Connect();
        void Subscribe(string publisherId);
        void Unsubscribe(string publisherId);
    }
}