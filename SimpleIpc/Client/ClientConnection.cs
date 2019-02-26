using log4net;
using SimpleIpc.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleIpc.Client
{
    public abstract class ClientConnection : Connection
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _port;
        private IPAddress _ipAddress;

        internal ClientConnection(string ipAddress, int port)
        {
            _port = port;

            if (ipAddress == null)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                _ipAddress = ipHostInfo.AddressList[0];
            }
            else
            {
                _ipAddress = IPAddress.Parse(ipAddress);
            }
        }

        protected void ConnectToServer()
        {
            Log.Debug("Establishing a connection");
            IPEndPoint remoteEP = new IPEndPoint(_ipAddress, _port);

            _socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(remoteEP);
        }

        internal void SendControl(ControlBytes controlByte, string message = null)
        {
            Log.Debug($"Sending control (byte = {controlByte}, message = {message})");
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
    }
}