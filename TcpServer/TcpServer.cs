using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public class TcpServer
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<string, Publisher> _publishers = new Dictionary<string, Publisher>();
        private static List<Subscriber> _subscribers = new List<Subscriber>();

        public static void StartListening(int port)
        {
            log.Info("Starting TcpServer");
            Subscriber.Publishers = _publishers;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                log.Info("Waiting for a connection");
                while (true)
                {
                    Socket socket = listener.Accept();
                    Task.Run(() => HandleNewConnection(socket));
                }
            }
            catch (Exception e)
            {
                log.Error("Failure in TcpServer", e);
            }
        }

        private static void HandleNewConnection(Socket socket)
        {
            log.Debug("Handling new connection");
            byte[] bytes = new Byte[1024];
            int bytesReceived;
            bytesReceived = socket.Receive(bytes);
            if (bytesReceived == 0)
            {
                log.Warn("Socket connection closed");
                socket.Dispose();
                return;
            }

            if (bytes[0] == (byte)ControlBytes.Escape)
            {
                if (bytes[1] == (byte)ControlBytes.RegisterPublisher)
                {
                    string publisherId = Encoding.ASCII.GetString(bytes, 2, bytesReceived - 2);
                    _publishers.Add(publisherId, new Publisher(socket));
                    log.Info($"New Publisher registered (ID = {publisherId})");
                }
                else if (bytes[1] == (byte)ControlBytes.RegisterSubscriber)
                {
                    _subscribers.Add(new Subscriber(socket));
                    log.Info("New Subscriber registered");
                }
                else
                {
                    log.Error("Non-registration control byte sent in first message");
                }
            }
            else
            {
                log.Error("Non-escape byte received in first message");
            }
        }
    }
}