using CommandLine;

namespace TcpServer
{
    class CommandLineOptions
    {
        [Option('p', "port", Default = 13001,
            HelpText = "Prints all messages to standard output.")]
        public int Port { get; set; }
    }
}