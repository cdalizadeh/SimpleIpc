using System;

namespace SimpleIpc.Server
{
    public interface IPublisher
    {
        IObservable<string> MessageReceived { get; }

        IObservable<string> PublishReceived {get;}
        IObservable<string> UnpublishReceived {get;}
    }
}