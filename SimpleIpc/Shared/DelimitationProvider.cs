using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleIpc.Shared
{
    /// <summary>
    /// A static class that provides data serialization and delimitation for TCP networking.
    /// </summary>
    internal static class DelimitationProvider
    {
        internal static byte[] Delimit(string data)
        {
            var bytes = new byte[data.Length + 1];
            Encoding.ASCII.GetBytes(data, 0, data.Length, bytes, 0);
            bytes[data.Length] = (byte)ControlBytes.Delimiter;
            return bytes;
        }

        internal static byte[] Delimit(string data, params byte[] prependedBytes)
        {
            var bytes = new byte[data.Length + prependedBytes.Length + 1];
            Encoding.ASCII.GetBytes(data, 0, data.Length, bytes, prependedBytes.Length);
            for (var i = 0; i < prependedBytes.Length; i++)
            {
                bytes[i] = prependedBytes[i];
            }
            bytes[bytes.Length - 1] = (byte)ControlBytes.Delimiter;
            return bytes;
        }

        internal static byte[] Delimit(params byte[] prependedBytes)
        {
            var bytes = new byte[prependedBytes.Length + 1];
            for (var i = 0; i < prependedBytes.Length; i++)
            {
                bytes[i] = prependedBytes[i];
            }
            bytes[bytes.Length - 1] = (byte)ControlBytes.Delimiter;
            return bytes;
        }

        internal static List<byte[]> Undelimit(ArraySegment<byte> messages)
        {
            const byte delimiter = (byte)ControlBytes.Delimiter;

            List<byte[]> splitMessages = new List<byte[]>();

            var lastDelimiterIndex = -1;
            var delimiterIndex = Array.FindIndex(messages.Array, messages.Offset, messages.Count, b => b == delimiter);
            while (delimiterIndex != -1)
            {
                var messageLength = delimiterIndex - lastDelimiterIndex - 1;
                var newMessage = new byte[messageLength];
                Array.Copy(messages.Array, lastDelimiterIndex + 1, newMessage, 0, messageLength);
                splitMessages.Add(newMessage);
                lastDelimiterIndex = delimiterIndex;
                delimiterIndex = Array.FindIndex(messages.Array, lastDelimiterIndex + 1, 
                    messages.Count - (lastDelimiterIndex + 1 - messages.Offset), b => b == delimiter);
            }
            return splitMessages;
        }
    }
}