using System;

namespace PubSubIpc.Shared
{
    public interface IPublisherClient
    {
        void Send(string message);
    }
}