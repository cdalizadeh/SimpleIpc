using System;
using System.Threading.Tasks;

namespace TcpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SubscriberClient sub = new SubscriberClient();
            sub.Connect();
            PublisherClient pub = new PublisherClient("pub05");
            pub.Connect();
            sub.Subscribe("pub05");
            sub.DataReceived.Subscribe((s) => Console.WriteLine("Received: " + s));
            var messageNum = 0;
            while (true)
            {
                pub.Send("message" + messageNum);
                messageNum++;
                Task.Delay(1000).Wait();
            }
            Console.ReadKey();
            //pub.Dispose();
        }
    }
}
