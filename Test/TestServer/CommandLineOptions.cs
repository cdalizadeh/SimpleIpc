using CommandLine;

namespace TestServer
{
    class CommandLineOptions
    {
        [Option('p', "publisher", Default = false,
            HelpText = "Creates a local publisher on the server")]
        public bool CreateLocalPublisher { get; set; }

        [Option('s', "subscriber", Default = false,
            HelpText = "Creates a local subscriber on the server")]
        public bool CreateLocalSubscriber { get; set; }
    }
}