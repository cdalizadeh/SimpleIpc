using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpClient
{
    public class PublisherClient : ClientConnection, IPublisherClient
    {
        private string _publisherId;

        public PublisherClient(string publisherId, int port = 13001) : base(port)
        {
            _publisherId = publisherId;
        }

        public void Connect()
        {
            EstablishConnection();
            BeginSending();
            SendControl(ControlBytes.RegisterPublisher, _publisherId);
        }

        public void Send(string message)
        {
            var encodedMsg = Encoding.ASCII.GetBytes(message);
            _sendDataSubject.OnNext(encodedMsg);
        }
    }
}