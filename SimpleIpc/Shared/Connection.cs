using System;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using log4net;

namespace SimpleIpc.Shared
{
    /// <summary>
    /// Abstract wrapper class around a Socket. Exposes IO as Rx Subjects.
    /// </summary>
    internal abstract class Connection : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const int MaxIncomingMessageLength = 1024;
        private volatile bool _disposed = false;
        private volatile bool _receiving = false;

        protected Socket _socket;
        protected Subject<byte[]> _dataReceivedSubject = new Subject<byte[]>();

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        public void SendData(byte[] bytes)
        {
            // Check for empty array.
            if (bytes.Length == 1) return;
            _socket.Send(bytes);
        }

        /// <summary>
        /// Starts the receive loop, enabling the socket to receive messages and publish them to <see cref="_dataReceivedSubject">
        /// </summary>
        public void InitReceive()
        {
            if (!_receiving)
            {
                Log.Info("Initializing receive loop");
                _receiving = true;
                Task.Run(() => ReceiveLoopAsync()); // Errors caught in ReceiveLoopAsync
            }
            else
            {
                Log.Warn($"{nameof(InitReceive)} called more than once");
            }
        }

        private async Task ReceiveLoopAsync()
        {
            byte[] receiveBuffer = new byte[MaxIncomingMessageLength];

            try
            {
                while (!_disposed)
                {
                    var receiveBufferSegment = new ArraySegment<byte>(receiveBuffer);

                    try
                    {
                        int numBytesReceived = await _socket.ReceiveAsync(receiveBufferSegment, SocketFlags.None);
                        receiveBufferSegment = receiveBufferSegment.Slice(0, numBytesReceived);

                        // Handle remote disconnection (Linux).
                        if (numBytesReceived == 0)
                        {
                            Log.Warn("Remote host disconnected");
                            Dispose();
                            break;
                        }

                        Log.Debug($"Received message ({numBytesReceived} bytes)");
                    }
                    catch(SocketException se)
                    {
                        // Handle socket error on disposal.
                        if (_disposed) break;
                        
                        // Handle remote disconnection (Windows).
                        if (se.ErrorCode == 10054)
                        {
                            Log.Warn("Remote host disconnected");
                            Dispose();
                            break;
                        }

                        // Handle other socket errors.
                        throw;
                    }
                    
                    var messages = DelimitationProvider.Undelimit(receiveBufferSegment);
                    foreach(var message in messages)
                    {
                        _dataReceivedSubject.OnNext(message);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error in receive loop", e);
            }
        }

        /// <inheritdoc />
        public void Dispose(){
            if (!_disposed)
            {
                Log.Info("Disposing connection");
                _disposed = true;
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Close();
                _dataReceivedSubject.OnCompleted();
                _dataReceivedSubject.Dispose();
            }
        }
    }
}