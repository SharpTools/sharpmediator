using System;

namespace SharpMediator {
    public interface IMediator {
        void Subscribe<T>(object subscriber, Action<T> action);
        void Unsubscribe<T>(object subscriber);
        void Publish(object message);
    }
}