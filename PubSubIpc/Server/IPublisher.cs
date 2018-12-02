using System;

namespace PubSubIpc.Server
{
    public interface IPublisher
    {
        IObservable<string> DataReceived { get; }
    }
}