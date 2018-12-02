using log4net;
using System;
using PubSubIpc.Server;

namespace TestServer
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            _log.Info("Starting test server");
            Server server = new Server();
            server.StartListening();
            
            _log.Info("Waiting");
            Console.ReadKey();        
        }
    }
}
