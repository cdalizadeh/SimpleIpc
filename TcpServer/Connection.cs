using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    public class Connection : IDisposable
    {
        private object _syncRoot = new object();
        private volatile bool _shutdown = false;
        private Socket _socket;

        public Connection(Socket socket)
        {
            _socket = socket;
            Task.Run(() => ReceiveLoop());
        }

        private void SendLoop()
        {

        }

        private void ReceiveLoop()
        {
            byte[] bytes;
            int bytesReceived;

            while (!_shutdown)
            {
                bytes = new Byte[1024];
                lock (_syncRoot)
                {
                    bytesReceived = _socket.Receive(bytes);
                }
                if (bytesReceived == 0)
                {
                    TerminateConnection("0 bytes received");
                    break;
                }
                
                // Check if bytes sent comply with ASCII
                if ((int)(bytes[0]) >= 0x80)
                {
                    Console.WriteLine("Escape sequence detected");
                }
                else
                {
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                    Console.WriteLine("Received: " + data);
                }
            }
        }

        private void TerminateConnection(string message)
        {
            lock (_syncRoot)
            {
                if (!_shutdown)
                {
                    _shutdown = true;
                    Console.WriteLine("Connection shutdown: " + message);
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                }
            }
        }

        public void Dispose(){
            TerminateConnection("call to Dispose() received");
        }
    }
}