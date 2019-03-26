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
        /// <param name="ipAddress">The IP address to connect to. Null indicates a connection to localhost</param>
        /// <param name="port">The port to connect to</param>
        public PublisherClient(string ipAddress = null, int port = 13001)
        {
            _connection = new ClientConnection(ipAddress, port);
            Log.Info("Creating new publisher");
        }

        /// <summary>
        /// Connects to the server at the specified port and IP address.
        /// </summary>
        public void Connect()
        {
            Log.Info("Connecting to server");
            _connection.ConnectToServer();
            Log.Info("Registering as publisher");
            _connection.InitSend();
            _connection.SendControl(ControlBytes.RegisterPublisher);
            Log.Info("Successfully connected to server and registered");
        }

        /// <inheritdoc />
        public void Send(string message)
        {
            Log.Info($"Sending message ({message})");
            _connection.SendData(message);
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