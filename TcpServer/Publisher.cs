using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpServer
{
    class Publisher : Connection, IPublisher
    {
        public Publisher(Connection connection) : base(connection.Socket)
        {

        }

        public void Test()
        {
            Console.WriteLine("Test");
            DataReceived.GetHashCode();
        }

        public void AddSubscriber(ISubscriber s)
        {

        }

        public void RemoveSubscriber(ISubscriber s)
        {

        }
    }
}