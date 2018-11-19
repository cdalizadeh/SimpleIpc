using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpServer
{
    class Subscriber : Connection, ISubscriber
    {
        public Subscriber(Connection connection) : base(connection.Socket)
        {

        }
    }
}