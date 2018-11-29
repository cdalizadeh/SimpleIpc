namespace PubSubIpc.Shared
{

    public enum ControlBytes : byte {Delimiter = (byte)0x81, Escape = (byte)0x80, RegisterSubscriber = (byte)0x43, RegisterPublisher = (byte)0x50, Subscribe = (byte)0x53, Unsubscribe = (byte)0x55};
}