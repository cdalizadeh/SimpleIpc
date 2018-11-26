using log4net;
using System;
using PubSubIpc.Server;

namespace TestServer
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log.Info("Starting test server");
            Server server = new Server();
            server.StartListening();
            
            log.Info("Waiting");
            Console.ReadKey();        
        }
    }
}
