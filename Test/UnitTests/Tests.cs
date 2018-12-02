using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using NUnit.Framework;
using PubSubIpc.Client;
using PubSubIpc.Server;

namespace Tests
{
    public class Tests
    {
        [Test]
        public async Task CreateServerTestAsync()
        {
            var server = new Server();
            server.StartListening();

            var publisher = new PublisherClient("abc123");
            publisher.Connect();
            publisher.InitSend();

            var subscriber = new SubscriberClient();
            subscriber.Connect();
            subscriber.InitReceive();
            subscriber.Subscribe("abc123");

            var receiveTask = subscriber.DataReceived.Take(1).ToTask();
            publisher.Send("hello");
            var result = await receiveTask;
            Assert.AreEqual("hello", result);
        }
    }
}