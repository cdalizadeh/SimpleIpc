using CommandLine;
using log4net;
using PubSubIpc.Client;
using System;

namespace TestClient
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(StartWithOptions);
        }

        private static void StartWithOptions(CommandLineOptions opts)
        {
            if (opts.ClientType == "publisher")
            {
                StartPublisherClient();
            }
            else if (opts.ClientType == "subscriber")
            {
                StartSubscriberClient();
            }
        }

        private static void StartPublisherClient()
        {
            log.Info("Starting test client as Publisher");
            PublisherClient publisher = new PublisherClient("pub21");
            publisher.Connect();

            log.Info("Starting client publisher loop");
            while (true)
            {
                var message = Console.ReadLine();
                if (message == "dispose")
                {
                    publisher.Dispose();
                }
                else
                {
                    publisher.Send(message);
                }
            }
        }

        private static void StartSubscriberClient()
        {
            log.Info("Starting test client as Subscriber");
            SubscriberClient subscriber = new SubscriberClient();
            subscriber.Connect();
            subscriber.Subscribe("pub21");
            subscriber.DataReceived.Subscribe((s) => log.Debug($"Received Message: {s}"));
            
            log.Info("Waiting");
            while (true)
            {
                var message = Console.ReadLine();
                if (message == "dispose")
                {
                    subscriber.Dispose();
                }
            }
        }
    }
}
