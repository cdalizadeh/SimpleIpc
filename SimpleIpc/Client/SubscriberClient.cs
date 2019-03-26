using System;
using log4net;
using SimpleIpc.Shared;

namespace SimpleIpc.Client
{
    /// <summary>
    /// A client that can connect to a remote server and subscribe to messages from it.
    /// </summary>
    public sealed class SubscriberClient : ISubscriberClient
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ClientConnection _connection;

        /// <inheritdoc />
        public IObservable<string> MessageReceived => _connection.MessageReceived;

        /// <summary>
        /// Creates a new <see cref="SubscriberClient">.
        /// </summary>
        /// <param name="ipAddress">The IP address to connect to. Null indicates a connection to localhost</param>
        /// <param name="port">The port to connect to</param>
        public SubscriberClient(string ipAddress = null, int port = 13001)
        {
            _connection = new ClientConnection(ipAddress, port);
            Log.Info("Creating new subscriber");
        }

        /// <summary>
        /// Connects to the server at the specified port and IP address.
        /// </summary>
        public void Connect()
        {
            Log.Info("Connecting to server");
            _connection.ConnectToServer();
            Log.Info("Registering as subscriber");
            _connection.InitSend();
            _connection.InitReceive();
            _connection.SendControl(ControlBytes.RegisterSubscriber);
            Log.Info("Successfully connected to server and registered");
        }

        /// <inheritdoc />
        public void Subscribe(string channelId)
        {
            Log.Info($"Subscribing to ({channelId})");
            _connection.SendControl(ControlBytes.Subscribe, channelId);
        }

        /// <inheritdoc />
        public void Unsubscribe(string channelId)
        {
            Log.Info($"Unsubscribing from ({channelId})");
            _connection.SendControl(ControlBytes.Unsubscribe, channelId);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}