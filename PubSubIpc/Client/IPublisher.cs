using System;

namespace PubSubIpc.Client
{
    interface IPublisher
    {
        void Connect();
        void Send(string message);
    }
}