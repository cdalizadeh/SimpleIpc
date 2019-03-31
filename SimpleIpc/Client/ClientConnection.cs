using log4net;
using SimpleIpc.Shared;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;

namespace SimpleIpc.Client
{
    /// <summary>
    /// A connection object to use within IPC clients
    /// </summary>
    internal sealed class ClientConnection : Connection
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _port;
        private IPAddress _ipAddress;

        public IObservable<string> MessageReceived => _dataReceivedSubject
            .Select(bytes => Encoding.ASCII.GetString(bytes, 0, bytes.Length));

        public ClientConnection(int port)
        {
            _port = port;
            _ipAddress = NetworkDefaults.LoopbackIPAddress;
        }

        public ClientConnection(IPAddress ipAddress, int port)
        {
            _port = port;
            _ipAddress = ipAddress;
        }

        public void ConnectToServer()
        {
            Log.Debug("Connecting to server");
            IPEndPoint remoteEP = new IPEndPoint(_ipAddress, _port);

            _socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(remoteEP);
        }

        public void SendControl(ControlBytes controlByte, string message = null)
        {
            if (message != null) Log.Debug($"Sending control (byte = {controlByte}, message = {message})");
            else Log.Debug($"Sending control (byte = {controlByte})");
            
            byte[] bytesToSend;
            if (message != null)
            {
                bytesToSend = DelimitationProvider.Delimit(message, (byte)ControlBytes.Escape, (byte)controlByte);
            }
            else
            {
                bytesToSend = DelimitationProvider.Delimit((byte)ControlBytes.Escape, (byte)(controlByte));
            }
            _sendDataSubject.OnNext(bytesToSend);
        }

        public void SendData(string data)
        {
            var delimitedData = DelimitationProvider.Delimit(data);
            _sendDataSubject.OnNext(delimitedData);
        }
    }
}