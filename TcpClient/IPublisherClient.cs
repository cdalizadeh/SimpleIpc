using System;

namespace TcpClient
{
    interface IPublisherClient
    {
        void Connect();
        void Send(string message);
    }
}