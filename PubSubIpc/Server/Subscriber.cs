using log4net;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace PubSubIpc.Server
{
    public abstract class Subscriber
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();

        private Subject<string> _dataReceivedSubject = new Subject<string>();

        protected IObservable<string> _dataReceived => _dataReceivedSubject;

        public static Dictionary<string, IPublisher> Publishers { get; set; }

        public void Subscribe(string publisherId)
        {
            _log.Info($"Subscribing to Publisher ({publisherId})");
            //check if publisher exists
            var publisher = Publishers[publisherId];
            _subscriptions[publisherId] = publisher.DataReceived.Subscribe(_dataReceivedSubject);
        }

        public void Unsubscribe(string publisherId)
        {
            _log.Info($"Unsubscribing from Publisher ({publisherId})");
            _subscriptions[publisherId].Dispose();
            _subscriptions.Remove(publisherId);
        }

        protected void UnsubscribeAll()
        {
            foreach(var subscription in _subscriptions)
            {
                subscription.Value.Dispose();
            }
            _subscriptions.Clear();
        }
    }
}
