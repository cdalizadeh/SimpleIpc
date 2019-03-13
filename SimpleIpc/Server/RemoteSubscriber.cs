using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using log4net;
using SimpleIpc.Shared;

namespace SimpleIpc.Server
{
    internal class RemoteSubscriber : ISubscriber
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ServerConnection _connection;

        private Subject<string> _messageSubject = new Subject<string>();

        public IObserver<string> MessageObserver => _messageSubject;
        public IObservable<string> SubscribeReceived {get;}
        public IObservable<string> UnsubscribeReceived {get;}

        public RemoteSubscriber(ServerConnection connection)
        {
            _connection = connection;

            SubscribeReceived = _connection.ControlReceived.Where((cc) => cc.Control == ControlBytes.Subscribe).Select((cc) => cc.Data);
            UnsubscribeReceived = _connection.ControlReceived.Where((cc) => cc.Control == ControlBytes.Unsubscribe).Select((cc) => cc.Data);

            _connection.InitSend();
            
            _messageSubject.Select(message => DelimitationProvider.Delimit(message)).Subscribe(_connection.SendData);
        }
    }
}