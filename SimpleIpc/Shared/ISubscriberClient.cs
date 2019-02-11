using System;

namespace SimpleIpc.Shared
{
    public interface ISubscriberClient
    {
        IObservable<string> MessageReceived {get;}

        void Subscribe(string channelId);
        void Unsubscribe(string channelId);

        void Dispose();
    }
}