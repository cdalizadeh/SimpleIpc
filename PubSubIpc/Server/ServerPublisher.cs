using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;

namespace PubSubIpc.Server
{
    class ServerPublisher
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Connection _connection;

        public IObservable<string> DataReceived => _connection.DataReceived;

        public ServerPublisher(Connection connection)
        {
            _connection = connection;
            _connection.DataReceived.Subscribe((s) => Console.WriteLine("Thing: " + s));
            _connection.BeginSending();
        }

        public void Test()
        {
            log.Error("Test");
        }
    }
}