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
        private volatile bool _shutdown = false;
        private object _syncRoot = new object();

        protected Socket _socket;
        protected Subject<byte[]> _sendDataSubject = new Subject<byte[]>();
        protected Subject<byte[]> _dataReceivedSubject = new Subject<byte[]>();

        public void InitSendLoop()
        {
            log.Info("Initializing send loop");
            Action<byte[]> onNext = (bytes) => 
            {
                _socket.Send(bytes);
                Task.Delay(10).Wait();
            };
            Action<Exception> onError = (e) => log.Error("Error in send loop", e);
            Action onCompleted = () => log.Debug("Send loop completed");
            _sendDataSubject.Subscribe(onNext, onError, onCompleted);
        }

        public void InitReceiveLoop()
        {
            log.Info("Initializing receive loop");
            new Thread(ReceiveLoop);
        }

        private async void ReceiveLoop()
        {
            byte[] bytes;
            ArraySegment<byte> bytesSegment;
            int bytesReceived;

            while (!_shutdown)
            {
                bytes = new byte[1024];
                bytesSegment = new ArraySegment<byte>(bytes);

                bytesReceived = await _socket.ReceiveAsync(bytesSegment, SocketFlags.None);
                if (bytesReceived == 0)
                {
                    log.Warn("Socket connection closed");
                    break;
                }
                
                _dataReceivedSubject.OnNext(bytesSegment.ToArray());
                log.Debug("Received message");
            }
        }

        private void TerminateConnection(string message)
        {
            lock (_syncRoot)
            {
                if (!_shutdown)
                {
                    _shutdown = true;
                    log.Warn($"Connection shutdown ({message})");
                    _socket?.Shutdown(SocketShutdown.Both);
                    _socket?.Close();
                    _dataReceivedSubject?.OnCompleted();
                    _dataReceivedSubject?.Dispose();
                    _sendDataSubject?.OnCompleted();
                    _sendDataSubject?.Dispose();
                }
            }
        }

        public void Dispose(){
            TerminateConnection("call to Dispose() received");
        }
    }
}