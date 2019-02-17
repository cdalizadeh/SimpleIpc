using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using log4net;
using SimpleIpc.Shared;

namespace SimpleIpc.Client
{
    public class SubscriberClient : ClientConnection, ISubscriberClient
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IObservable<string> MessageReceived => _dataReceivedSubject
            .Select(bytes => Encoding.ASCII.GetString(bytes, 0, bytes.Length));

        public SubscriberClient(string ipAddress = null, int port = 13001) : base(ipAddress, port)
        {
            Log.Info("Creating new subscriber");
        }

        public void Connect()
        {
            Log.Info("Connecting to server");
            ConnectToServer();
            InitSend();
            InitReceive();
            SendControl(ControlBytes.RegisterSubscriber);
            Log.Info("Successfully connected to server");
        }

        public void Subscribe(string channelId)
        {
            Log.Info($"Subscribing to ({channelId})");
            SendControl(ControlBytes.Subscribe, channelId);
        }

        public void Unsubscribe(string channelId)
        {
            Log.Info($"Unsubscribing from ({channelId})");
            SendControl(ControlBytes.Unsubscribe, channelId);
        }
    }
}