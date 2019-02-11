using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;

namespace SimpleIpc.Server
{
    class RemotePublisher : IPublisher
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ServerConnection _connection;

        public IObservable<string> DataReceived => _connection.DataReceived;

        public RemotePublisher(ServerConnection connection)
        {
            _connection = connection;
            _connection.DataReceived.Subscribe((s) => Log.Debug("Received: " + s));
        }
    }
}