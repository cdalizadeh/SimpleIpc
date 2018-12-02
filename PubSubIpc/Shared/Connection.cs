using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using log4net;

namespace PubSubIpc.Shared
{
    /// <summary>
    /// Abstract wrapper class around a Socket. Exposes IO as Rx Subjects. Delimits messages.
    /// </summary>
    public abstract class Connection : IDisposable
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const int _maxIncomingMessageLength = 1024;
        private volatile bool _disposed = false;
        private volatile bool _receiving = false;
        private volatile bool _sending = false;

        protected Socket _socket;
        protected Subject<byte[]> _sendDataSubject = new Subject<byte[]>();
        protected Subject<byte[]> _dataReceivedSubject = new Subject<byte[]>();

        /// <summary>
        /// Creates a socket subscription to _sendDataSubject, enabling Connection to send data
        /// </summary>
        public void InitSend()
        {
            if (!_sending)
            {
                _log.Info("Initializing send loop");
                _sending = true;
                Action<byte[]> onNext = (bytes) => 
                {
                    // Add delimiter to byte array before sending.
                    var delimitedBytes = new byte[bytes.Length + 1];
                    Array.Copy(bytes, delimitedBytes, bytes.Length);
                    delimitedBytes[bytes.Length] = (byte)ControlBytes.Delimiter;
                    _socket.Send(delimitedBytes);
                };
                Action<Exception> onError = (e) => _log.Error("Error in send subscription", e);
                Action onCompleted = () => _log.Debug("Send subscription completed");
                _sendDataSubject.Subscribe(onNext, onError, onCompleted);
            }
            else
            {
                _log.Warn($"{nameof(InitSend)} called more than once");
            }
        }

        /// <summary>
        /// Starts the receive loop, enabling the connection socket to receive messages and publish them to _dataReceivedSubject
        /// </summary>
        public void InitReceive()
        {
            if (!_receiving)
            {
                _log.Info("Initializing receive loop");
                _receiving = true;
                Task.Run(() => ReceiveLoopAsync());
            }
            else
            {
                _log.Warn($"{nameof(InitReceive)} called more than once");
            }
        }

        /// <summary>
        /// Runs a socket receive loop. Kicked off by InitReceive()
        /// </summary>
        /// <returns>The async completion task</returns>
        private async Task ReceiveLoopAsync()
        {
            byte[] receiveBuffer;
            byte[] receivedMessage;
            ArraySegment<byte> bytesSegment;
            int numBytesReceived;

            try
            {
                while (!_disposed)
                {
                    receiveBuffer = new byte[_maxIncomingMessageLength];
                    bytesSegment = new ArraySegment<byte>(receiveBuffer);

                    try
                    {
                        numBytesReceived = await _socket.ReceiveAsync(bytesSegment, SocketFlags.None);
                    }
                    catch(SocketException se)
                    {
                        // Handle socket error on disposal.
                        if (_disposed) break;
                        
                        // Handle remote disconnection (Windows).
                        if (se.ErrorCode == 10054)
                        {
                            _log.Warn("Remote host disconnected");
                            Dispose();
                            break;
                        }

                        // Handle other socket errors.
                        throw;
                    }

                    // Handle remote disconnection (Linux).
                    if (numBytesReceived == 0)
                    {
                        _log.Warn("Remote host disconnected");
                        Dispose();
                        break;
                    }

                    _log.Debug($"Received message ({numBytesReceived} bytes)");

                    receivedMessage = bytesSegment.ToArray();

                    var messages = SplitMessage(receivedMessage, (byte)ControlBytes.Delimiter);
                    foreach(var message in messages)
                    {
                        _dataReceivedSubject.OnNext(message);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Error in receive loop", e);
            }
        }

        /// <summary>
        /// Splits an array of bytes by a delimiter
        /// </summary>
        /// <param name="delimitedMessages">A byte array of messages separated by the delimiter</param>
        /// <param name="delimiter">A byte that delimits messages</param>
        /// <returns>A list of byte arrays, each an individual message</returns>
        private List<byte[]> SplitMessage(byte[] delimitedMessages, byte delimiter)
        {
            List<byte[]> messages = new List<byte[]>();

            var lastDelimiterIndex = -1;
            var delimiterIndex = Array.FindIndex(delimitedMessages, b => b == delimiter);
            while (delimiterIndex != -1)
            {
                var messageLength = delimiterIndex - lastDelimiterIndex - 1;
                var newMessage = new byte[messageLength];
                Array.Copy(delimitedMessages, lastDelimiterIndex + 1, newMessage, 0, messageLength);
                messages.Add(newMessage);
                lastDelimiterIndex = delimiterIndex;
                delimiterIndex = Array.FindIndex(delimitedMessages, lastDelimiterIndex + 1, b => b == delimiter);
            }
            return messages;
        }

        /// <summary>
        /// Safely disposes of the Connection object
        /// </summary>
        public void Dispose(){
            if (!_disposed)
            {
                _log.Info("Disposing connection");
                _disposed = true;
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Close();
                _dataReceivedSubject.OnCompleted();
                _dataReceivedSubject.Dispose();
                _sendDataSubject.OnCompleted();
                _sendDataSubject.Dispose();
            }
        }
    }
}