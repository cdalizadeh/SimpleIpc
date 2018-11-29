using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PubSubIpc.Shared;

namespace PubSubIpc.Client
{
    public class Publisher : ClientConnection, IPublisher
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _publisherId;

        public Publisher(string publisherId, int port = 13001) : base(port)
        {
            log.Info($"Creating new publisher (publisherId = {publisherId})");
            _publisherId = publisherId;
        }

        public void Connect()
        {
            log.Info("Connecting to server");
            ConnectToServer();
            log.Info("Registering as publisher");
            InitSend();
            SendControl(ControlBytes.RegisterPublisher, _publisherId);
            log.Info("Successfully connected and registered");
        }

        public void Send(string message)
        {
            log.Debug($"Sending message ({message})");
            var encodedMsg = Encoding.ASCII.GetBytes(message);
            _sendDataSubject.OnNext(encodedMsg);
        }
    }
}