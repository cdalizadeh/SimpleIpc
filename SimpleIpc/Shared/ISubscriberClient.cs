using System;

namespace SimpleIpc.Shared
{
    public interface ISubscriberClient
    {
        IObservable<string> DataReceived {get;}

        void Subscribe(string publisherId);
        void Unsubscribe(string publisherId);
    }
}