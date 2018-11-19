using System;
using System.Reactive;
using System.Reactive.Linq;
using CommandLine;

namespace TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(StartWithOptions);
        }

        private static void StartWithOptions(CommandLineOptions opts)
        {
            TcpServer.StartListening(opts.Port);
        }
    }
}
