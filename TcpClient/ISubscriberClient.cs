using System;

namespace TcpClient
{
    interface ISubscriberClient : IDisposable
    {
        IObservable<string> DataReceived {get;}
        void Connect();
        void Subscribe(string publisherId);
        void Unsubscribe(string publisherId);
    }
}