using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace PubSubIpc.Server
{
    public class Connection : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Socket _socket;
        private object _syncRoot = new object();
        private Subject<string> _sendDataSubject = new Subject<string>();
        private Subject<string> _dataReceivedSubject = new Subject<string>();
        private Subject<ControlCommand> _controlReceivedSubject = new Subject<ControlCommand>();


        public volatile bool _shutdown = false;
        public IObserver<string> SendData => (IObserver<string>)_sendDataSubject;
        public IObservable<string> DataReceived => (IObservable<string>)_dataReceivedSubject;
        public IObservable<ControlCommand> ControlReceived => (IObservable<ControlCommand>)_controlReceivedSubject;

        public Connection(Socket socket)
        {
            _socket = socket;
        }

        public void BeginSending()
        {
            log.Info("Sending enabled");
            Action<string> onNext = (message) =>
            {
                var encodedMsg = Encoding.ASCII.GetBytes(message);
                _socket.Send(encodedMsg);
            };
            Action<Exception> onError = (e) => log.Error("Error in send observable", e);
            _sendDataSubject.Subscribe(onNext, onError);
        }

        public void BeginReceiving()
        {
            Task.Run(() => ReceiveLoop());
        }

        private void ReceiveLoop()
        {
            log.Info("Receiving enabled");
            byte[] bytes;
            int bytesReceived;

            while (!_shutdown)
            {
                bytes = new Byte[1024];
                bytesReceived = _socket.Receive(bytes);
                if (bytesReceived == 0)
                {
                    log.Warn("Socket connection closed");
                    break;
                }
                
                // Check if escape byte is present.
                if (bytes[0] == (byte)ControlBytes.Escape)
                {
                    log.Debug($"Received control command ({bytes[1]})");
                    // Get control byte.
                    byte control = bytes[1];
                    // Get data.
                    string data = Encoding.ASCII.GetString(bytes, 2, bytesReceived - 2);
                    // Send to observable.
                    var command = new ControlCommand{Control = (ControlBytes)control, Data = data};
                    _controlReceivedSubject.OnNext(command);
                }
                else
                {
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                    log.Debug($"Received data ({data})");
                    _dataReceivedSubject.OnNext(data);
                }
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
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
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