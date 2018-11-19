using System;
using System.Threading.Tasks;

namespace TcpServer
{
    interface IConnection : IDisposable
    {
        IObservable<string> DataReceived {get;}
        IObservable<ControlCommand> ControlReceived {get;}

        Task<string> GetNextMessageAsync();
        Task<ControlCommand> GetNextControlAsync();

        void StartReceiving();

        //void Send(string message);
    }
}