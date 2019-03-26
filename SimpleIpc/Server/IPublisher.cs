using System;

namespace SimpleIpc.Server
{
    /// <summary>
    /// An internal server representation of a publisher.
    /// </summary>
    internal interface IPublisher
    {
        IObservable<string> MessageReceived { get; }

        IObservable<string> PublishReceived {get;}
        IObservable<string> UnpublishReceived {get;}
    }
}