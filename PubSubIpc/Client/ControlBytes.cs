namespace PubSubIpc.Client
{
    public enum ControlBytes : byte {Escape = (byte)0x80, RegisterSubscriber = (byte)0x43, RegisterPublisher = (byte)0x50, Subscribe = (byte)0x53, Unsubscribe = (byte)0x55};
}