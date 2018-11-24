using System;

namespace PubSubIpc
{
    interface IPublisher
    {
        void Connect();
        void Send(string message);
    }
}