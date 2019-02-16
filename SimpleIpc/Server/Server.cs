using log4net;
using SimpleIpc.Shared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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

        private Dictionary<string, Channel> _channels = new Dictionary<string, Channel>();

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
                    if (!_channels.ContainsKey(channelId))
                    {
                        _channels[channelId] = new Channel();
                    }

                    _channels[channelId].AddPublisher(publisher);
                }
            };

            publisher.PublishReceived.Subscribe(onPublishReceived);

            Action<string> OnUnpublishReceived = (channelId) =>
            {
                lock (_syncRoot)
                {
                    if (!_channels.ContainsKey(channelId))
                    {
                        Log.Error($"channel specified in unpublish request does not exist (channel = {channelId})");
                        return;
                    }

                    var channel = _channels[channelId];
                    channel.RemovePublisher(publisher);

                    if (channel.IsEmpty)
                    {
                        _channels.Remove(channelId);
                    }
                }
            };

            publisher.UnpublishReceived.Subscribe(OnUnpublishReceived);
        }

        private void InitializeISubscriber(ISubscriber subscriber)
        {
            Action<string> onSubscribeReceived = (channelId) =>
            {
                lock (_syncRoot)
                {
                    if (!_channels.ContainsKey(channelId))
                    {
                        _channels[channelId] = new Channel();
                    }

                    _channels[channelId].AddSubscriber(subscriber);
                }
            };

            subscriber.SubscribeReceived.Subscribe(onSubscribeReceived);

            Action<string> onUnsubscribeReceived = (channelId) =>
            {
                lock (_syncRoot)
                {
                    if (!_channels.ContainsKey(channelId))
                    {
                        Log.Error($"channel specified in unsubscribe request does not exist (channel = {channelId})");
                        return;
                    }

                    var channel = _channels[channelId];
                    channel.RemoveSubscriber(subscriber);

                    if (channel.IsEmpty)
                    {
                        _channels.Remove(channelId);
                    }
                }
            };

            subscriber.UnsubscribeReceived.Subscribe(onUnsubscribeReceived);
        }
    }
}