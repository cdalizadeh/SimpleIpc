using System;
using log4net;
using SimpleIpc.Shared;

namespace SimpleIpc.Client
{
    public class SubscriberClient : ISubscriberClient
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ClientConnection _connection;

        public IObservable<string> MessageReceived => _connection.MessageReceived;

        public SubscriberClient(string ipAddress = null, int port = 13001)
        {
            _connection = new ClientConnection(ipAddress, port);
            Log.Info("Creating new subscriber");
        }

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

        public void Subscribe(string channelId)
        {
            Log.Info($"Subscribing to ({channelId})");
            _connection.SendControl(ControlBytes.Subscribe, channelId);
        }

        public void Unsubscribe(string channelId)
        {
            Log.Info($"Unsubscribing from ({channelId})");
            _connection.SendControl(ControlBytes.Unsubscribe, channelId);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}