using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public class TcpServer
    {
        private static Dictionary<string, Publisher> _publishers = new Dictionary<string, Publisher>();
        private static List<Subscriber> _subscribers = new List<Subscriber>();

        public static void StartListening(int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");
                while (true)
                {
                    Socket socket = listener.Accept();
                    Task.Run(() => HandleConnection(socket));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void HandleConnection(Socket socket)
        {
            var connection = new Connection(socket);
            connection.GetNextControlAsync().ContinueWith((t)=>
            {
                if (t.IsCanceled)
                {
                    Console.WriteLine("Socket is cancelled");
                    connection.Dispose();
                }
                else if (t.IsFaulted)
                {
                    Console.WriteLine("Socket is in error");
                    connection.Dispose();
                }
                else
                {
                    var command = t.Result;
                    if (command.Control == 'P')
                    {
                        var publisherId = command.Data;
                        _publishers.Add(publisherId, new Publisher(connection));
                    }
                    else if (command.Control == 'S')
                    {
                        _subscribers.Add(new Subscriber(connection));
                    }
                    else
                    {
                        Console.WriteLine("Unknown control detected");
                    }
                }
            });
            connection.StartReceiving();
        }
    }
}