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
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IObservable<string> DataReceived => _dataReceivedSubject
            .Select(bytes => Encoding.ASCII.GetString(bytes, 0, bytes.Length));

        public SubscriberClient(int port = 13001) : base(port)
        {
            _log.Info("Creating new subscriber");
        }

        public void Connect()
        {
            _log.Info("Connecting to server");
            ConnectToServer();
            InitSend();
            InitReceive();
            SendControl(ControlBytes.RegisterSubscriber);
            _log.Info("Successfully connected to server");
        }

        public void Subscribe(string publisherId)
        {
            _log.Info($"Subscribing to ({publisherId})");
            SendControl(ControlBytes.Subscribe, publisherId);
        }

        public void Unsubscribe(string publisherId)
        {
            _log.Info($"Unsubscribing from ({publisherId})");
            SendControl(ControlBytes.Unsubscribe, publisherId);
        }
    }
}