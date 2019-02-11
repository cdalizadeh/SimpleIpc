using CommandLine;
using log4net;
using System;
using SimpleIpc.Server;
using SimpleIpc.Shared;

namespace TestServer
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static IPublisherClient _localPublisher;
        private static ISubscriberClient _localSubscriber;

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(StartWithOptions);
        }

        private static void StartWithOptions(CommandLineOptions opts)
        {
            Log.Info("Starting test server");
            Server server = new Server();
            server.StartListening();

            if (opts.CreateLocalPublisher)
            {
                _localPublisher = server.CreateLocalPublisher("pub21");
            }

            if (opts.CreateLocalSubscriber)
            {
                _localSubscriber = server.CreateLocalSubscriber();
                _localSubscriber.Subscribe("pub21");
                _localSubscriber.DataReceived.Subscribe((s) => Log.Debug($"Received Message: {s}"));
            }

            Log.Info("Test server ready");
            if (opts.CreateLocalPublisher)
            {
                while (true)
                {
                    var message = Console.ReadLine();
                    if (message == "dispose")
                    {
                        _localPublisher.Dispose();
                    }
                    else
                    {
                        _localPublisher.Send(message);
                    }
                }
            }
            else
            {
                Console.ReadKey();
            }
        }
    }
}
