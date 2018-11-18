using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpClient
{
    public class TcpClient
    {
        public static void StartClient()
        {
            Console.WriteLine("Starting TCP Client");
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 13001);

                try
                {
                    Socket sender = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    while(true)
                    {
                        Console.WriteLine("Enter text to send:");
                        var msg = Console.ReadLine();
                        var encodedMsg = Encoding.ASCII.GetBytes(msg);

                        int bytesSent = sender.Send(encodedMsg);
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}",ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}",se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine( e.ToString());
            }
        }
    }
}
