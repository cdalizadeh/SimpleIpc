using CommandLine;

namespace TestClient
{
    class CommandLineOptions
    {
        [Option('t', "type", Default = "publisher",
            HelpText = "The type of client (publisher, subscriber) to create.")]
        public string ClientType { get; set; }
    }
}