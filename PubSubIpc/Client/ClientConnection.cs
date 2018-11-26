using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace PubSubIpc.Client
{
    public abstract class ClientConnection : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int _port;
        private Socket _socket;
        private object _syncRoot = new object();

        protected volatile bool _shutdown = false;
        protected Subject<string> _dataReceivedSubject = new Subject<string>();
        protected Subject<byte[]> _sendDataSubject = new Subject<byte[]>();

        protected ClientConnection(int port = 13001)
        {
            _port = port;
        }

        protected void EstablishConnection()
        {
            log.Debug("Establishing a connection");
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

            _socket = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(remoteEP);
        }

        protected void InitSending()
        {
            log.Debug("Sending enabled");
            Action<byte[]> onNext = (bytes) => 
            {
                _socket.Send(bytes);
                Task.Delay(10).Wait();
            };
            Action<Exception> onError = (e) => log.Error("Error in Send stream", e);
            _sendDataSubject.Subscribe(onNext, onError);
        }

        protected void InitReceiving()
        {
            log.Debug("Receiving enabled");
            Task.Run(() => ReceiveLoop());
        }

        protected void SendControl(ControlBytes controlByte, string message = null)
        {
            log.Debug($"Sending control (byte = {controlByte}, message = {message})");
            byte[] bytesToSend;
            if (message != null)
            {
                List<byte> encodedMessage = Encoding.ASCII.GetBytes(message).ToList();
                bytesToSend = encodedMessage.Prepend((byte)controlByte).Prepend((byte)ControlBytes.Escape).ToArray();
            }
            else
            {
                bytesToSend = new byte[]{(byte)ControlBytes.Escape, (byte)controlByte};
            }
            _sendDataSubject.OnNext(bytesToSend);
        }

        private void ReceiveLoop()
        {
            byte[] bytes;
            int bytesReceived;

            while (!_shutdown)
            {
                bytes = new Byte[1024];
                
                bytesReceived = _socket.Receive(bytes);
                if (bytesReceived == 0)
                {
                    TerminateConnection("0 bytes received");
                    break;
                }

                string data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                _dataReceivedSubject.OnNext(data);
                log.Debug("Received message: " + data);
            }
        }

        private void TerminateConnection(string message)
        {
            lock (_syncRoot)
            {
                if (!_shutdown)
                {
                    log.Warn("Connection shutdown: " + message);
                    _shutdown = true;
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