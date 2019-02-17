using CommandLine;

namespace TestClient
{
    class CommandLineOptions
    {
        [Option('t', "type", Default = "publisher",
            HelpText = "The type of client (publisher, subscriber) to create.")]
        public string ClientType { get; set; }

        [Option('i', "IP address", Default = null,
            HelpText = "The remote IP address to connect to")]
        public string IpAddress { get; set; }

        [Option('p', "port", Default = 13001,
            HelpText = "The remote IP address to connect to")]
        public int Port { get; set; }
    }
}