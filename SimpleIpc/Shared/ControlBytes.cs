namespace SimpleIpc.Shared
{
    /// <summary>
    /// Significant bytes with use in program control. All control bytes lie outside of the ASCII range.
    /// </summary>
    internal enum ControlBytes : byte
    {
        /// <summary>
        /// Delimits individual data blocks being transmitted.
        /// </summary>
        Delimiter = (byte)0x80,

        /// <summary>
        /// Indicates that the current data block is a control block, as opposed to a message block. Must be followed by another control byte.
        /// </summary>
        Escape = (byte)0x81,

        /// <summary>
        /// Registers a connection as a subscriber.
        /// </summary>
        RegisterSubscriber = (byte)0x90,

        /// <summary>
        /// Registers a connection as a publisher.
        /// </summary>
        RegisterPublisher = (byte)0x91,

        /// <summary>
        /// Indicates subscription to a channel. Must be followed by the channel ID.
        /// </summary>
        Subscribe = (byte)0x92,

        /// <summary>
        /// Indicates unsubscription from a channel. Must be followed by the channel ID.
        /// </summary>
        Unsubscribe = (byte)0x93,

        /// <summary>
        /// Indicates publication to a channel. Must be followed by the channel ID.
        /// </summary>
        Publish = (byte)0x94,

        /// <summary>
        /// Indicates unpublication from a channel. Must be followed by the channel ID.
        /// </summary>
        Unpublish = (byte)0x95
    };
}