using log4net;
using SimpleIpc.Shared;

namespace SimpleIpc.Client
{
    public class PublisherClient : IPublisherClient
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ClientConnection _connection;

        public PublisherClient(string ipAddress = null, int port = 13001)
        {
            _connection = new ClientConnection(ipAddress, port);
            Log.Info("Creating new publisher");
        }

        public void Connect()
        {
            Log.Info("Connecting to server");
            _connection.ConnectToServer();
            Log.Info("Registering as publisher");
            _connection.InitSend();
            _connection.SendControl(ControlBytes.RegisterPublisher);
            Log.Info("Successfully connected to server and registered");
        }

        public void Send(string message)
        {
            Log.Info($"Sending message ({message})");
            _connection.SendData(message);
        }

        public void Publish(string channelId)
        {
            Log.Info($"Publishing to ({channelId})");
            _connection.SendControl(ControlBytes.Publish, channelId);
        }

        public void Unpublish(string channelId)
        {
            Log.Info($"Unpublishing from ({channelId})");
            _connection.SendControl(ControlBytes.Unpublish, channelId);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}