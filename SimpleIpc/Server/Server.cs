using log4net;
using SimpleIpc.Shared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIpc.Server
{
    public class Server
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _port;
        private Socket _listener;
        private bool _listening = false;
        private Dictionary<string, IPublisher> _publishers = new Dictionary<string, IPublisher>();
        private List<ISubscriber> _subscribers = new List<ISubscriber>();

        public Server(int port = 13001)
        {
            Log.Info("Creating new server");
            _port = port;

            Subscriber.Publishers = _publishers;
        }

        public void StartListening()
        {
            if (!_listening)
            {
                Log.Info("Listening for connections");
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
                Log.Warn($"{nameof(StartListening)} called more than once");
            }
        }

        public ISubscriberClient CreateLocalSubscriber()
        {
            Log.Info("Creating local subscriber client");
            var subscriber = new LocalSubscriberClient();
            _subscribers.Add(subscriber);

            return subscriber;
        }

        public IPublisherClient CreateLocalPublisher(string publisherId)
        {
            Log.Info("Creating local publisher client");
            var publisher = new LocalPublisherClient();
            _publishers.Add(publisherId, publisher);

            return publisher;
        }

        private async void StartReceiveLoop()
        {
            Log.Info("Waiting for a connection");
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
                Log.Debug("Handling new connection");
                var serverConnection = new ServerConnection(socket);
                var registrationTask = serverConnection.ControlReceived.Take(1).ToTask();
                serverConnection.InitReceive();
                var registration = await registrationTask;
                if (registration.Control == ControlBytes.RegisterPublisher)
                {
                    var publisherId = registration.Data;
                    CreateRemotePublisher(publisherId, serverConnection);
                }
                else if (registration.Control == ControlBytes.RegisterSubscriber)
                {
                    _subscribers.Add(new RemoteSubscriber(serverConnection));
                    Log.Info("New Subscriber registered");
                }
                else
                {
                    Log.Error("Non-registration control byte sent in first message");
                }
            }
            catch(Exception e)
            {
                Log.Error("Failed to handle new connection", e);
            }
        }

        private void CreateRemotePublisher(string publisherId, ServerConnection serverConnection)
        {
            var publisher = new RemotePublisher(serverConnection);
            _publishers.Add(publisherId, publisher);

            Action onCompleted = () =>
            {
                _publishers.Remove(publisherId);
            };
            publisher.DataReceived.Subscribe((s)=>{}, onCompleted);
            
            Log.Info($"New Publisher registered (ID = {publisherId})");
        }

        private void CreateRemoteSubscriber(string subscriberId)
        {

        }
    }
}