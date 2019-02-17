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
                StartPublisherClient(opts);
            }
            else if (opts.ClientType == "subscriber")
            {
                StartSubscriberClient(opts);
            }
        }

        private static void StartPublisherClient(CommandLineOptions opts)
        {
            Log.Info("Starting test client as Publisher");
            PublisherClient publisher = new PublisherClient(opts.IpAddress, opts.Port);
            publisher.Connect();
            publisher.Publish("test-channel");

            Log.Info("Starting client publisher loop");
            while (true)
            {
                var newLine = Console.ReadLine();
                var newLineSplit = newLine.Split(' ');

                if (newLineSplit[0] == "dispose")
                {
                    publisher.Dispose();
                    break;
                }
                else if (newLineSplit[0] == "publish" && newLineSplit.Length > 1)
                {
                    publisher.Publish(newLineSplit[1]);
                }
                else if (newLineSplit[0] == "unpublish" && newLineSplit.Length > 1)
                {
                    publisher.Unpublish(newLineSplit[1]);
                }
                else
                {
                    publisher.Send(newLine);
                }
            }
            Log.Info("Terminating PublisherClient test program");
        }

        private static void StartSubscriberClient(CommandLineOptions opts)
        {
            Log.Info("Starting test client as Subscriber");
            SubscriberClient subscriber = new SubscriberClient(opts.IpAddress, opts.Port);
            subscriber.Connect();
            subscriber.Subscribe("test-channel");
            subscriber.MessageReceived.Subscribe((s) => Log.Debug($"Received Message: {s}"));
            
            Log.Info("Waiting");
            while (true)
            {
                var newLine = Console.ReadLine();
                var newLineSplit = newLine.Split(' ');

                if (newLineSplit[0] == "dispose")
                {
                    subscriber.Dispose();
                    break;
                }
                else if (newLineSplit[0] == "subscribe" && newLineSplit.Length > 1)
                {
                    subscriber.Subscribe(newLineSplit[1]);
                }
                else if (newLineSplit[0] == "unsubscribe" && newLineSplit.Length > 1)
                {
                    subscriber.Unsubscribe(newLineSplit[1]);
                }
            }
            Log.Info("Terminating SubscriberClient test program");
        }
    }
}
