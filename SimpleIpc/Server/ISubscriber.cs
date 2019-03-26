using System;

namespace SimpleIpc.Server
{
    /// <summary>
    /// An internal server representation of a subscriber.
    /// </summary>
    internal interface ISubscriber
    {
        IObserver<string> MessageObserver {get;}

        IObservable<string> SubscribeReceived {get;}
        IObservable<string> UnsubscribeReceived {get;}
    }
}