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
        
        private object _syncRoot = new object();
        private readonly int _port;
        private Socket _listener;
        private bool _listening = false;

        private Dictionary<string, List<IPublisher>> _publishers = new Dictionary<string, List<IPublisher>>();
        private Dictionary<string, List<ISubscriber>> _subscribers = new Dictionary<string, List<ISubscriber>>();

        public Server(int port = 13001)
        {
            Log.Info("Creating new server");
            _port = port;
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
                    CreateRemotePublisher(serverConnection);
                }
                else if (registration.Control == ControlBytes.RegisterSubscriber)
                {
                    CreateRemoteSubscriber(serverConnection);
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

        private void CreateRemotePublisher(ServerConnection serverConnection)
        {
            var publisher = new RemotePublisher(serverConnection);
            InitializeIPublisher(publisher);
        }

        private void CreateRemoteSubscriber(ServerConnection serverConnection)
        {
            var subscriber = new RemoteSubscriber(serverConnection);
            InitializeISubscriber(subscriber);
        }

        public ISubscriberClient CreateLocalSubscriber()
        {
            Log.Info("Creating local subscriber client");
            var subscriber = new LocalSubscriberClient();
            InitializeISubscriber(subscriber);

            return subscriber;
        }

        public IPublisherClient CreateLocalPublisher()
        {
            Log.Info("Creating local publisher client");
            var publisher = new LocalPublisherClient();
            InitializeIPublisher(publisher);

            return publisher;
        }

        private void InitializeIPublisher(IPublisher publisher)
        {
            Action<string> onPublishReceived = (channelId) =>
            {
                lock (_syncRoot)
                {
                    // Register publisher with the given channelId.
                    if (!_publishers.ContainsKey(channelId))
                    {
                        _publishers[channelId] = new List<IPublisher>();
                    }
                    
                    List<IPublisher> publishersList = _publishers[channelId];
                    if (publishersList.Contains(publisher))
                    {
                        throw new Exception($"publisher is already publishing to channel {channelId}");
                    }
                    publishersList.Add(publisher);

                    // Link publisher to existing subscribers.
                    if (_subscribers.ContainsKey(channelId))
                    {
                        var subscribers = _subscribers[channelId];
                        foreach (var subscriber in subscribers)
                        {
                            publisher.MessageReceived.Subscribe(subscriber.MessageObserver);
                        }
                    }
                }
            };

            publisher.PublishReceived.Subscribe(onPublishReceived);
        }

        private void InitializeISubscriber(ISubscriber subscriber)
        {
            Action<string> onSubscribeReceived = (channelId) =>
            {
                lock (_syncRoot)
                {
                    // Register subscriber with the given channelId.
                    if (!_subscribers.ContainsKey(channelId))
                    {
                        _subscribers[channelId] = new List<ISubscriber>();
                    }

                    List<ISubscriber> subscribersList = _subscribers[channelId];
                    if (subscribersList.Contains(subscriber))
                    {
                        throw new Exception($"subscriber is already subscribing to channel {channelId}");
                    }
                    subscribersList.Add(subscriber);

                    // Link subscriber to existing publishers.
                    if (_publishers.ContainsKey(channelId))
                    {
                        var publishers = _publishers[channelId];
                        foreach (var publisher in publishers)
                        {
                            publisher.MessageReceived.Subscribe(subscriber.MessageObserver);
                        }
                    }
                }
            };

            subscriber.SubscribeReceived.Subscribe(onSubscribeReceived);
        }
    }
}