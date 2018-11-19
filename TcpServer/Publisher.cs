using System;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;

namespace TcpServer
{
    class Publisher : Connection
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        new public IObservable<string> DataReceived => base.DataReceived;

        public Publisher(Socket socket) : base(socket)
        {
            //DataReceived.Subscribe((s) => Console.WriteLine("Thing: " + s));
            BeginSending();
        }

        public void Test()
        {
            log.Error("Test");
        }
    }
}