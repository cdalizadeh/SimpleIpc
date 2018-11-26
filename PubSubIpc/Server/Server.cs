using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace PubSubIpc.Server
{
    public class Server : IServer
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _port;
        private Dictionary<string, ServerPublisher> _publishers = new Dictionary<string, ServerPublisher>();
        private List<ServerSubscriber> _subscribers = new List<ServerSubscriber>();

        public Server(int port = 13001)
        {
            log.Info("Creating new server");
            _port = port;
        }

        public void StartListening()
        {
            log.Info("Starting Server");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _port);

            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                log.Info("Waiting for a connection");
                while (true)
                {
                    Socket socket = listener.Accept();
                    HandleNewConnection(socket);
                }
            }
            catch (Exception e)
            {
                log.Error("Failure in Server", e);
            }
        }

        private async void HandleNewConnection(Socket socket)
        {
            log.Debug("Handling new connection");
            var connection = new Connection(socket);
            var firstControl = await connection.ControlReceived.ToTask();
            connection.BeginReceiving();
            if (firstControl.Control == ControlBytes.RegisterPublisher)
            {
                var publisherId = firstControl.Data;
                _publishers.Add(publisherId, new ServerPublisher(connection));
                log.Info($"New Publisher registered (ID = {publisherId})");
            }
            else if (firstControl.Control == ControlBytes.RegisterSubscriber)
            {
                _subscribers.Add(new ServerSubscriber(connection));
                log.Info("New Subscriber registered");
            }
            else
            {
                log.Error("Non-registration control byte sent in first message");
            }
        }

        public LocalSubscriber CreateLocalSubscriber()
        {
            return new LocalSubscriber();
        }

        public LocalPublisher CreateLocalPublisher()
        {
            return new LocalPublisher();
        }
    }
}