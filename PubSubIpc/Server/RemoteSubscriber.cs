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
    class RemoteSubscriber : Subscriber, ISubscriber
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IDisposable _subscription;
        private readonly ServerConnection _connection;

        public RemoteSubscriber(ServerConnection connection)
        {
            _connection = connection;

            Action<ControlCommand> onNextControlCommand = (cc) =>
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
                    _log.Error("Unknown control byte");
                }
            };
            Action onCompletedControlCommand = () =>
            {
                Dispose();
            };
            _subscription = _connection.ControlReceived.Subscribe(onNextControlCommand, onCompletedControlCommand);

            _connection.InitSend();
            _dataReceived.Select(message => Encoding.ASCII.GetBytes(message)).Subscribe(_connection.SendData);
        }

        public void Dispose()
        {
            _subscription.Dispose();
            UnsubscribeAll();
        }
    }
}