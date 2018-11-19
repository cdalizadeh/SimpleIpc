using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpClient
{
    public class SubscriberClient : ClientConnection, ISubscriberClient
    {
        public IObservable<string> DataReceived => (IObservable<string>) _dataReceivedSubject;

        public SubscriberClient(int port = 13001) : base(port)
        {
        }

        public void Connect()
        {
            Console.WriteLine("starting connect");
            EstablishConnection();
            BeginSending();
            BeginReceiving();
            SendControl(ControlBytes.RegisterSubscriber);
            Console.WriteLine("ending connect");
        }

        public void Subscribe(string publisherId)
        {
            Console.WriteLine("Starting subscribe");
            SendControl(ControlBytes.Subscribe, publisherId);
            Console.WriteLine("Ending subscribe");
        }

        public void Unsubscribe(string publisherId)
        {
            SendControl(ControlBytes.Unsubscribe, publisherId);
        }
    }
}