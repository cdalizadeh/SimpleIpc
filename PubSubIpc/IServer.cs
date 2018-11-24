using System;

namespace PubSubIpc
{
    interface IServer
    {
        void StartListening();
        LocalSubscriber CreateLocalSubscriber();
        LocalPublisher CreateLocalPublisher();
    }
}