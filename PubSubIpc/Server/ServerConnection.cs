using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using PubSubIpc.Shared;

namespace PubSubIpc.Server
{
    public class ServerConnection : Connection
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IObserver<byte[]> SendData => _sendDataSubject;
        public IObservable<string> DataReceived => _dataReceivedSubject
            .Where(bytes => bytes[0] != (byte)ControlBytes.Escape)
            .Select(bytes => Encoding.ASCII.GetString(bytes, 0, bytes.Length));
        public IObservable<ControlCommand> ControlReceived => _dataReceivedSubject
            .Where(bytes => bytes[0] == (byte)ControlBytes.Escape)
            .Select(bytes => ControlCommand.FromByteArray(bytes));

        public ServerConnection(Socket socket)
        {
            _socket = socket;
        }
    }
}