using System;

namespace SimpleIpc.Shared
{
    public interface IPublisherClient
    {
        void Send(string message);
    }
}