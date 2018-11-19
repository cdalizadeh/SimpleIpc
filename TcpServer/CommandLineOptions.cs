using CommandLine;

namespace TcpServer
{
    class CommandLineOptions
    {
        [Option('p', "port", Default = 13001,
            HelpText = "The TCP port for the application to communicate over.")]
        public int Port { get; set; }
    }
}