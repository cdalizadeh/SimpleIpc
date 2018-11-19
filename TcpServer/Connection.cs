using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    public class Connection : IConnection
    {
        #region private fields
        private object _syncRoot = new object();
        private volatile bool _shutdown = false;
        private Socket _socket;
        private Subject<ControlCommand> _controlReceivedSubject = new Subject<ControlCommand>();
        private Subject<string> _dataReceivedSubject = new Subject<string>();
        #endregion

        #region public properties
        public Socket Socket => _socket;
        public IObservable<string> DataReceived => (IObservable<string>) _dataReceivedSubject;
        public IObservable<ControlCommand> ControlReceived => (IObservable<ControlCommand>) _controlReceivedSubject;
        #endregion

        public Connection(Socket socket)
        {
            _socket = socket;
        }

        public void StartReceiving()
        {
            Task.Run(() => ReceiveLoop());
        }

        public Task<string> GetNextMessageAsync()
        {
            var tcs = new TaskCompletionSource<string>();
            Action<string> onNext = (s)=>tcs.SetResult(s);
            Action onCompleted = ()=>tcs.SetCanceled();
            Action<Exception> onError = (e)=>tcs.SetException(e);
            
            DataReceived.Subscribe(onNext, onError, onCompleted);
            return tcs.Task;
        }

        public Task<ControlCommand> GetNextControlAsync()
        {
            var tcs = new TaskCompletionSource<ControlCommand>();
            Action<ControlCommand> onNext = (s)=>tcs.SetResult(s);
            Action onCompleted = ()=>tcs.SetCanceled();
            Action<Exception> onError = (e)=>tcs.SetException(e);
            
            ControlReceived.Subscribe(onNext, onError, onCompleted);
            return tcs.Task;
        }

        private void SendLoop()
        {

        }

        private void ReceiveLoop()
        {
            byte[] bytes;
            int bytesReceived;

            while (!_shutdown)
            {
                bytes = new Byte[1024];
                lock (_syncRoot)
                {
                    bytesReceived = _socket.Receive(bytes);
                }
                if (bytesReceived == 0)
                {
                    TerminateConnection("0 bytes received");
                    break;
                }
                
                // Check if escape byte is present
                if ((int)(bytes[0]) >= 0x80)
                {
                    // Get control byte.
                    char control = Encoding.ASCII.GetString(bytes, 1, 1).ToCharArray()[0];
                    // Get data.
                    string data = Encoding.ASCII.GetString(bytes, 2, bytesReceived - 2);
                    // Send to observable.
                    var command = new ControlCommand{Control = control, Data = data};
                    _controlReceivedSubject.OnNext(command);
                }
                else
                {
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
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
                    Console.WriteLine("Connection shutdown: " + message);
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                }
            }
        }

        public void Dispose(){
            TerminateConnection("call to Dispose() received");
        }
    }
}