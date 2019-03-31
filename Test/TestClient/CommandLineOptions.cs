using CommandLine;

namespace TestClient
{
    class CommandLineOptions
    {
        [Option('t', "Type", Default = "publisher",
            HelpText = "The type of client (publisher, subscriber) to create")]
        public string ClientType { get; set; }

        [Option('h', "Hostname", Default = "localhost",
            HelpText = "The remote hostname (or IP address) to connect to")]
        public string Hostname { get; set; }

        [Option('p', "Port", Default = 13001,
            HelpText = "The remote port to connect to")]
        public int Port { get; set; }
    }
}