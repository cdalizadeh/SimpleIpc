using System;
using System.Threading.Tasks;

namespace TcpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            PublisherClient pub = new PublisherClient("pub05");
            pub.Connect();
            pub.Send("message1");
            pub.Send("message2");
            pub.Dispose();
        }
    }
}
