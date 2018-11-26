using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace PubSubIpc.Client
{
    public class Subscriber : ClientConnection, ISubscriber
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IObservable<string> DataReceived => (IObservable<string>) _dataReceivedSubject;

        public Subscriber(int port = 13001) : base(port)
        {
            log.Info("Creating new subscriber");
        }

        public void Connect()
        {
            log.Info("Connecting to server");
            EstablishConnection();
            InitSending();
            InitReceiving();
            SendControl(ControlBytes.RegisterSubscriber);
            log.Info("Successfully connected to server");
        }

        public void Subscribe(string publisherId)
        {
            Console.WriteLine("Starting subscribe");
            SendControl(ControlBytes.Subscribe, publisherId);
        }

        public void Unsubscribe(string publisherId)
        {
            Console.WriteLine("Starting unsubscribe");
            SendControl(ControlBytes.Unsubscribe, publisherId);
        }
    }
}