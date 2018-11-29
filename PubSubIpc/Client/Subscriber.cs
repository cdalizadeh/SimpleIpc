using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PubSubIpc.Shared;

namespace PubSubIpc.Client
{
    public class Subscriber : ClientConnection, ISubscriber
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IObservable<string> DataReceived => _dataReceivedSubject
            .Select(bytes => Encoding.ASCII.GetString(bytes, 0, bytes.Length));

        public Subscriber(int port = 13001) : base(port)
        {
            log.Info("Creating new subscriber");
        }

        public void Connect()
        {
            log.Info("Connecting to server");
            ConnectToServer();
            InitSendLoop();
            InitReceiveLoop();
            SendControl(ControlBytes.RegisterSubscriber);
            log.Info("Successfully connected to server");
        }

        public void Subscribe(string publisherId)
        {
            log.Info($"Subscribing to ({publisherId})");
            SendControl(ControlBytes.Subscribe, publisherId);
        }

        public void Unsubscribe(string publisherId)
        {
            log.Info($"Unsubscribing from ({publisherId})");
            SendControl(ControlBytes.Unsubscribe, publisherId);
        }
    }
}