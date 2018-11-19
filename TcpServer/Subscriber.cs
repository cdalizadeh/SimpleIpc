using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

namespace TcpServer
{
    class Subscriber : Connection
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();

        public static Dictionary<string, Publisher> Publishers;


        public Subscriber(Socket socket) : base(socket)
        {
            Action<ControlCommand> onNext = (cc) =>
            {
                if (cc.Control == ControlBytes.Subscribe)
                {
                    Subscribe(cc.Data);
                }
                else if (cc.Control == ControlBytes.Unsubscribe)
                {
                    Unsubscribe(cc.Data);
                }
                else
                {
                    log.Error("Unknown control byte");
                }
            };
            ControlReceived.Subscribe(onNext);
            BeginReceiving();
            BeginSending();
        }

        public void Subscribe(string publisherId)
        {
            log.Info($"Subscribing to Publisher ({publisherId})");
            //check if publisher exists
            var publisher = Publishers[publisherId];
            _subscriptions[publisherId] = publisher.DataReceived.Subscribe((IObserver<string>)SendData);
        }

        public void Unsubscribe(string publisherId)
        {
            log.Info($"Unsubscribing from Publisher ({publisherId})");
            _subscriptions[publisherId].Dispose();
            _subscriptions.Remove(publisherId);
        }
    }
}