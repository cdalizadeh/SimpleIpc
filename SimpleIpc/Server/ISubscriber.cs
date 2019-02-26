using System;

namespace SimpleIpc.Server
{
    internal interface ISubscriber
    {
        IObserver<string> MessageObserver {get;}

        IObservable<string> SubscribeReceived {get;}
        IObservable<string> UnsubscribeReceived {get;}
    }
}