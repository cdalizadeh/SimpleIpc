using NUnit.Framework;
using SimpleIpc.Client;
using SimpleIpc.Server;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public async Task CreateServerTestAsync()
        {
            var server = new Server();
            server.StartListening();

            var publisher = new PublisherClient();
            publisher.Connect();
            publisher.Publish("test-channel");

            var subscriber = new SubscriberClient();
            subscriber.Connect();
            subscriber.Subscribe("test-channel");

            var receiveTask = subscriber.MessageReceived.Take(1).ToTask(new CancellationTokenSource(5000).Token);
            publisher.Send("hello");
            var result = await receiveTask;
            Assert.AreEqual("hello", result);
        }
    }
}