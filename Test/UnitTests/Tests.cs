using NUnit.Framework;
using SimpleIpc.Client;
using SimpleIpc.Server;
using SimpleIpc.Shared;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        [Ignore("Not working yet")]
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

        [Test]
        public void DelimitationProviderTest()
        {
            var data = "TEST-MESSAGE";
            var dataBytes = Encoding.ASCII.GetBytes(data);

            var delimitedData = DelimitationProvider.Delimit(data);
            var delimitedDataSegment = new ArraySegment<byte>(delimitedData);
            var unDelimitedData = DelimitationProvider.Undelimit(delimitedDataSegment)[0];

            Assert.True(unDelimitedData.SequenceEqual(dataBytes));

        }
    }
}