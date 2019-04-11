using CommandLine;
using log4net;
using System;
using SimpleIpc.Server;
using SimpleIpc.Shared;
using System.Net;

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
            IPAddress ipAddress;
            try
            {
                ipAddress = IPAddress.Parse(opts.Hostname);
            }
            catch(Exception)
            {
                ipAddress = Dns.GetHostEntry(opts.Hostname).AddressList[0];
            }
            Server server = new Server(ipAddress);
            server.StartListening();

            if (opts.CreateLocalPublisher)
            {
                _localPublisher = server.CreateLocalPublisher();
                _localPublisher.Publish("test-channel");
            }

            if (opts.CreateLocalSubscriber)
            {
                _localSubscriber = server.CreateLocalSubscriber();
                _localSubscriber.Subscribe("test-channel");
                _localSubscriber.MessageReceived.Subscribe((s) => Log.Info($"LocalSubscriber received message: {s}"));
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
