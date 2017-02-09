using System;
using System.Reflection;

namespace SharpMediator {
    public class WeakDelegate : WeakReference {
        private MethodInfo _method;
        private WeakReference _actionTarget;

        public WeakDelegate(object actionOwner, Delegate action)
            : base(actionOwner) {
            _method = action.GetMethodInfo();
            _actionTarget = new WeakReference(action.Target);
        }

        public void Execute(object message) {
            if (!IsAlive) {
                return;
            }
            _method.Invoke(_actionTarget.Target, new[] { message });
        }
    }
}