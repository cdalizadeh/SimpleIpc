using log4net;
using System;
using PubSubIpc.Client;

namespace TestClient
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log.Info("Starting test client");
            Publisher publisher = new Publisher("pub21");

            Console.WriteLine("Waiting");
            Console.ReadKey();
        }
    }
}
