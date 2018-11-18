using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    public class TcpServer
    {
        public static void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");
                while (true)
                {
                    Socket handler = listener.Accept();
                    Task.Run(() => {HandleConnection(handler);});
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void HandleConnection(Socket s)
        {
            Console.WriteLine("New Connection");
            while (true)
            {
                byte[] bytes = new Byte[1024];
                int bytesRec = s.Receive(bytes);
                if (bytesRec == 0)
                {
                    Console.WriteLine("0 bytes received. Terminating connection");
                    break;
                }
                string data = Encoding.ASCII.GetString(bytes,0,bytesRec);
                Console.WriteLine("Received: " + data);
            }
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }
    }
}