using System;
using System.Text;
using log4net;
using SimpleIpc.Shared;

namespace SimpleIpc.Client
{
    public class PublisherClient : ClientConnection, IPublisherClient
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _publisherId;

        public PublisherClient(string publisherId, int port = 13001) : base(port)
        {
            _log.Info($"Creating new publisher (publisherId = {publisherId})");
            _publisherId = publisherId;
        }

        public void Connect()
        {
            _log.Info("Connecting to server");
            ConnectToServer();
            _log.Info("Registering as publisher");
            InitSend();
            SendControl(ControlBytes.RegisterPublisher, _publisherId);
            _log.Info("Successfully connected and registered");
        }

        public void Send(string message)
        {
            _log.Debug($"Sending message ({message})");
            var encodedMsg = Encoding.ASCII.GetBytes(message);
            _sendDataSubject.OnNext(encodedMsg);
        }
    }
}