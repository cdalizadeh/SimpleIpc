using System;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using log4net;

namespace SimpleIpc.Shared
{
    /// <summary>
    /// Abstract wrapper class around a Socket. Exposes IO as Rx Subjects. Delimits messages.
    /// </summary>
    public abstract class Connection : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
        internal void InitSend()
        {
            if (!_sending)
            {
                Log.Info("Initializing send loop");
                _sending = true;
                Action<byte[]> onNext = (bytes) => 
                {
                    // Check for empty string.
                    if (bytes.Length == 1) return;

                    _socket.Send(bytes);
                };
                Action<Exception> onError = (e) => Log.Error("Error in send subscription", e);
                Action onCompleted = () => Log.Info("Send subscription completed");
                _sendDataSubject.Subscribe(onNext, onError, onCompleted);
            }
            else
            {
                Log.Warn($"{nameof(InitSend)} called more than once");
            }
        }

        /// <summary>
        /// Starts the receive loop, enabling the connection socket to receive messages and publish them to _dataReceivedSubject
        /// </summary>
        internal void InitReceive()
        {
            if (!_receiving)
            {
                Log.Info("Initializing receive loop");
                _receiving = true;
                Task.Run(() => ReceiveLoopAsync());
            }
            else
            {
                Log.Warn($"{nameof(InitReceive)} called more than once");
            }
        }

        /// <summary>
        /// Runs a socket receive loop. Kicked off by InitReceive()
        /// </summary>
        /// <returns>The async completion task</returns>
        private async Task ReceiveLoopAsync()
        {
            byte[] receiveBuffer = new byte[_maxIncomingMessageLength];

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

        /// <summary>
        /// Safely disposes of the Connection object
        /// </summary>
        public void Dispose(){
            if (!_disposed)
            {
                Log.Info("Disposing connection");
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