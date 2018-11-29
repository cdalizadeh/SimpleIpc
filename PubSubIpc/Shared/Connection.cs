using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using PubSubIpc.Shared;

namespace PubSubIpc.Shared
{
    public abstract class Connection : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const int _maxIncomingMessageLength = 1024;
        private volatile bool _disposed = false;
        private volatile bool _receiving = false;
        private volatile bool _sending = false;

        protected Socket _socket;
        protected Subject<byte[]> _sendDataSubject = new Subject<byte[]>();
        protected Subject<byte[]> _dataReceivedSubject = new Subject<byte[]>();

        public void InitSendLoop()
        {
            if (!_sending)
            {
                log.Info("Initializing send loop");
                _sending = true;
                Action<byte[]> onNext = (bytes) => 
                {
                    _socket.Send(bytes);
                    Task.Delay(10).Wait();
                };
                Action<Exception> onError = (e) => log.Error("Error in send loop", e);
                Action onCompleted = () => log.Debug("Send loop completed");
                _sendDataSubject.Subscribe(onNext, onError, onCompleted);
            }
            else
            {
                log.Warn($"{nameof(InitSendLoop)} called more than once");
            }
        }

        public void InitReceiveLoop()
        {
            if (!_receiving)
            {
                log.Info("Initializing receive loop");
                _receiving = true;
                Task.Run(() => ReceiveLoopAsync());
            }
            else
            {
                log.Warn($"{nameof(InitReceiveLoop)} called more than once");
            }
        }

        private async Task ReceiveLoopAsync()
        {
            byte[] bytes;
            ArraySegment<byte> bytesSegment;
            int bytesReceived;

            try
            {
                while (!_disposed)
                {
                    bytes = new byte[_maxIncomingMessageLength];
                    bytesSegment = new ArraySegment<byte>(bytes);

                    try
                    {
                        bytesReceived = await _socket.ReceiveAsync(bytesSegment, SocketFlags.None);
                    }
                    catch(SocketException se)
                    {
                        // Handle socket error on disposal.
                        if (_disposed) break;
                        
                        // Handle remote disconnection (Windows).
                        if (se.ErrorCode == 10054)
                        {
                            log.Warn("Remote host disconnected");
                            Dispose();
                            break;
                        }

                        // Handle other socket errors.
                        throw;
                    }

                    // Handle remote disconnection (Linux).
                    if (bytesReceived == 0)
                    {
                        log.Warn("Remote host disconnected");
                        Dispose();
                        break;
                    }

                    log.Debug($"Received message ({bytesReceived} bytes)");
                    _dataReceivedSubject.OnNext(bytesSegment.ToArray());
                }
            }
            catch (Exception e)
            {
                log.Error("Error in receive loop", e);
            }
        }

        public void Dispose(){
            if (!_disposed)
            {
                log.Info("Disposing connection");
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