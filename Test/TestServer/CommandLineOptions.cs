using CommandLine;

namespace TestServer
{
    class CommandLineOptions
    {
        [Option('p', "Publisher", Default = false,
            HelpText = "Creates a local publisher on the server")]
        public bool CreateLocalPublisher { get; set; }

        [Option('s', "Subscriber", Default = false,
            HelpText = "Creates a local subscriber on the server")]
        public bool CreateLocalSubscriber { get; set; }

        [Option('h', "Hostname", Default = "localhost",
            HelpText = "The hostname (or IP address) to create the server on")]
        public string Hostname { get; set; }
    }
}