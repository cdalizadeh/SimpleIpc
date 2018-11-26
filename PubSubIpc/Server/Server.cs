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
        private Socket _listener;
        private Dictionary<string, ServerPublisher> _publishers = new Dictionary<string, ServerPublisher>();
        private List<ServerSubscriber> _subscribers = new List<ServerSubscriber>();

        public Server(int port = 13001)
        {
            log.Info("Creating new server");
            _port = port;

            ServerSubscriber.Publishers = _publishers;
        }

        public void StartListening()
        {
            log.Info("Starting Server");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _port);

            _listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            
            _listener.Bind(localEndPoint);
            _listener.Listen(10);

            Task.Run(()=>StartReceiveLoop());
        }

        private void StartReceiveLoop()
        {
            try
            {
                log.Info("Waiting for a connection");
                while (true)
                {
                    Socket socket = _listener.Accept();
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
            var registrationTask = connection.ControlReceived.Take(1).ToTask();
            connection.InitReceiving();
            var registration = await registrationTask;
            if (registration.Control == ControlBytes.RegisterPublisher)
            {
                var publisherId = registration.Data;
                _publishers.Add(publisherId, new ServerPublisher(connection));
                log.Info($"New Publisher registered (ID = {publisherId})");
            }
            else if (registration.Control == ControlBytes.RegisterSubscriber)
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