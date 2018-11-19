using System;
using System.Threading.Tasks;

namespace TcpServer
{
    interface IPublisher : IConnection
    {
        void AddSubscriber(ISubscriber s);

        void RemoveSubscriber(ISubscriber s);
    }
}