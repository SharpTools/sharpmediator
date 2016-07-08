using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMediator {
    public class Mediator : IMediator {
        private static object _sync = new object();
        private Dictionary<Type, List<WeakDelegate>> _subscribers = new Dictionary<Type, List<WeakDelegate>>();

        public static IMediator Default = new Mediator();

        public void Subscribe<T>(object subscriber, Action<T> action, SubscriptionKind subscriptionKind) {
            lock (_sync) {
                EnsureSubscribersList<T>();
                _subscribers[typeof(T)].Add(new WeakDelegate(subscriber, action));
            }
        }

        public void Subscribe<T>(object subscriber, Action<T> action) {
            Subscribe(subscriber, action, SubscriptionKind.Direct);
        }

        private void EnsureSubscribersList<T>() {
            if (!_subscribers.ContainsKey(typeof(T))) {
                _subscribers.Add(typeof(T), new List<WeakDelegate>());
            }
        }

        public void Publish(object message) {
            lock (_sync) {
                var type = message.GetType();
                if (!_subscribers.ContainsKey(type)) return;
                var alive = _subscribers[type].Where(x => x.IsAlive).ToList();
                foreach (var weakAction in alive) {
                    weakAction.Execute(message);
                }
                _subscribers[type] = alive;
            }
        }

        public void Unsubscribe<T>(object subscriber) {
            lock (_sync) {
                var type = typeof (T);
                if (!_subscribers.ContainsKey(type)) return;
                foreach (var wd in _subscribers[type].Where(wd => wd.Target == subscriber)) {
                    wd.Target = null;
                }
            }
        }

        public int RegisteredSubscribers() {
            lock (_sync) {
                return _subscribers.Sum(x => x.Value?.Count ?? 0);
            }
        }
    }
}