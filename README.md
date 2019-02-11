# SimpleIpc

SimpleIpc is a basic Interprocess Communication library written in C# for .NET Core. The library allows developers to create publishers that can pass messages over TCP to subscribers in another process. The interfaces for `PublisherClient` and `SubscriberClient` are quite simple:

```csharp
namespace SimpleIpc.Shared
{
    public interface IPublisherClient
    {
        void Send(string message);

        void Publish(string channelId);
        void Unpublish(string channelId);

        void Dispose();
    }

    public interface ISubscriberClient
    {
        IObservable<string> MessageReceived {get;}

        void Subscribe(string channelId);
        void Unsubscribe(string channelId);

        void Dispose();
    }
}
```
SimpleIpc exposes data received on the Subscriber as a ReactiveX Observable. The server makes use of ReactiveX internally to manage messages and connections.

To try SimpleIpc for yourself:
1. Clone the repository
2. `cd Test/TestServer && dotnet run`
3. In a new terminal, `cd Test/TestClient && dotnet run -- -t publisher`
4. In one last terminal, `cd Test/TestClient && dotnet run -- -t subscriber`
5. The subscriber process should now receive all messages sent by the publisher