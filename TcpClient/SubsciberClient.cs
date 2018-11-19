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
            EstablishConnection();
            BeginSending();
            BeginReceiving();
            SendControl(ControlBytes.RegisterSubscriber);
        }

        public void Subscribe(string publisherId)
        {
            SendControl(ControlBytes.Subscribe, publisherId);
        }

        public void Unsubscribe(string publisherId)
        {
            SendControl(ControlBytes.Unsubscribe, publisherId);
        }
    }
}