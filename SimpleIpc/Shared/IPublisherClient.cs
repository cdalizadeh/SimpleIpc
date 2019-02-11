using System;

namespace SimpleIpc.Shared
{
    public interface IPublisherClient
    {
        void Send(string message);

        void Publish(string channelId);
        void Unpublish(string channelId);

        void Dispose();
    }
}