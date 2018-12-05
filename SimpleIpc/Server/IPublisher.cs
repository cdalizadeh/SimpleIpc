using System;

namespace SimpleIpc.Server
{
    public interface IPublisher
    {
        IObservable<string> DataReceived { get; }
    }
}