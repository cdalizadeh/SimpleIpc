using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PubSubIpc.Shared;

namespace PubSubIpc.Server
{
    class ServerSubscriber
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();
        private readonly ServerConnection _connection;

        public static Dictionary<string, ServerPublisher> Publishers;


        public ServerSubscriber(ServerConnection connection)
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
            _connection = connection;
            _connection.ControlReceived.Subscribe(onNext);
            _connection.InitSend();
        }

        public void Subscribe(string publisherId)
        {
            log.Info($"Subscribing to Publisher ({publisherId})");
            //check if publisher exists
            var publisher = Publishers[publisherId];
            _subscriptions[publisherId] = publisher.DataReceived
                .Select(message => Encoding.ASCII.GetBytes(message)).Subscribe(_connection.SendData);
        }

        public void Unsubscribe(string publisherId)
        {
            log.Info($"Unsubscribing from Publisher ({publisherId})");
            _subscriptions[publisherId].Dispose();
            _subscriptions.Remove(publisherId);
        }
    }
}