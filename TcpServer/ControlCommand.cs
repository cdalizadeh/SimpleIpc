namespace TcpServer
{
    public struct ControlCommand
    {
        public ControlBytes Control {get; set;}
        public string Data {get; set;}
    }
}