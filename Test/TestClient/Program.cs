using CommandLine;
using log4net;
using SimpleIpc.Client;
using System;

namespace TestClient
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            Log.Info("Starting test client as Publisher");
            PublisherClient publisher = new PublisherClient();
            publisher.Connect();
            publisher.Publish("test-channel");

            Log.Info("Starting client publisher loop");
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
            Log.Info("Starting test client as Subscriber");
            SubscriberClient subscriber = new SubscriberClient();
            subscriber.Connect();
            subscriber.Subscribe("test-channel");
            subscriber.MessageReceived.Subscribe((s) => Log.Debug($"Received Message: {s}"));
            
            Log.Info("Waiting");
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
