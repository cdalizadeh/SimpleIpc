using System.Net;
using log4net;
using SimpleIpc.Shared;

namespace SimpleIpc.Client
{
    /// <summary>
    /// A client that can connect to a remote IPC server and publish messages to it.
    /// </summary>
    public sealed class PublisherClient : IPublisherClient
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ClientConnection _connection;

        /// <summary>
        /// Creates a new <see cref="PublisherClient">.
        /// </summary>
        /// <param name="ipAddress">The IP address to connect to</param>
        /// <param name="port">The port to connect to</param>
        public PublisherClient(IPAddress ipAddress, int port = NetworkDefaults.DefaultPort)
        {
            Log.Info($"Creating new publisher (IPAdress = {ipAddress}, port = {port})");
            _connection = new ClientConnection(ipAddress, port);
        }

        /// <summary>
        /// Creates a new <see cref="PublisherClient"> that connects to the loopback network interface.
        /// </summary>
        /// <param name="port">The port to connect to</param>
        public PublisherClient(int port = NetworkDefaults.DefaultPort)
        {
            Log.Info($"Creating new publisher (IPAdress = localhost, port = {port})");
            _connection = new ClientConnection(port);
        }

        /// <summary>
        /// Connects to the server at the specified port and IP address.
        /// </summary>
        public void Connect()
        {
            Log.Info("Connecting to server");
            _connection.ConnectToServer();
            Log.Info("Registering as publisher");
            _connection.SendControl(ControlBytes.RegisterPublisher);
            Log.Info("Successfully connected to server and registered");
        }

        /// <inheritdoc />
        public void Send(string message)
        {
            Log.Info($"Sending message ({message})");
            _connection.SendMessage(message);
        }

        /// <inheritdoc />
        public void Publish(string channelId)
        {
            Log.Info($"Publishing to ({channelId})");
            _connection.SendControl(ControlBytes.Publish, channelId);
        }

        /// <inheritdoc />
        public void Unpublish(string channelId)
        {
            Log.Info($"Unpublishing from ({channelId})");
            _connection.SendControl(ControlBytes.Unpublish, channelId);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}