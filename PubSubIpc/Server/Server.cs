using log4net;
using PubSubIpc.Shared;
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
    public class Server
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _port;
        private Socket _listener;
        private bool _listening = false;
        private Dictionary<string, IPublisher> _publishers = new Dictionary<string, IPublisher>();
        private List<ISubscriber> _subscribers = new List<ISubscriber>();

        public Server(int port = 13001)
        {
            log.Info("Creating new server");
            _port = port;

            Subscriber.Publishers = _publishers;
        }

        public void StartListening()
        {
            if (!_listening)
            {
                log.Info("Listening for connections");
                _listening = true;

                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _port);

                _listener = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                _listener.Bind(localEndPoint);
                _listener.Listen(10);

                Task.Run(() => StartReceiveLoop());
            }
            else
            {
                log.Warn($"{nameof(StartListening)} called more than once");
            }
        }

        private async void StartReceiveLoop()
        {
            log.Info("Waiting for a connection");
            while (true)
            {
                Socket socket = await _listener.AcceptAsync();

                #pragma warning disable 4014
                Task.Run(() => HandleNewConnection(socket));
                #pragma warning restore 4014
            }
        }

        private async Task HandleNewConnection(Socket socket)
        {
            try
            {
                log.Debug("Handling new connection");
                var connection = new ServerConnection(socket);
                var registrationTask = connection.ControlReceived.Take(1).ToTask();
                connection.InitReceive();
                var registration = await registrationTask;
                if (registration.Control == ControlBytes.RegisterPublisher)
                {
                    var publisherId = registration.Data;
                    _publishers.Add(publisherId, new RemotePublisher(connection));
                    log.Info($"New Publisher registered (ID = {publisherId})");
                }
                else if (registration.Control == ControlBytes.RegisterSubscriber)
                {
                    _subscribers.Add(new RemoteSubscriber(connection));
                    log.Info("New Subscriber registered");
                }
                else
                {
                    log.Error("Non-registration control byte sent in first message");
                }
            }
            catch(Exception e)
            {
                log.Error("Failed to handle new connection", e);
            }
        }

        public ISubscriberClient CreateLocalSubscriber()
        {
            var subscriber = new LocalSubscriberClient();
            _subscribers.Add(subscriber);

            return subscriber;
        }

        public IPublisherClient CreateLocalPublisher(string publisherId)
        {
            var publisher = new LocalPublisherClient();
            _publishers.Add(publisherId, publisher);

            return publisher;
        }
    }
}