using System;

namespace PubSubIpc.Server
{
    interface IServer
    {
        void StartListening();
        LocalSubscriber CreateLocalSubscriber();
        LocalPublisher CreateLocalPublisher();
    }
}