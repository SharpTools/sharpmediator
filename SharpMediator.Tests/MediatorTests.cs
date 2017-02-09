using System;
using Xunit;

namespace SharpMediator.Tests {
    public class MediatorTests {

        private Mediator _mediator;
        private Sub _sub;

        public MediatorTests() {
            _mediator = new Mediator();
            _sub = new Sub();
        }
        [Fact]
        public void Should_register_msgA() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            _mediator.Publish(new MsgA());
            Assert.Equal(1, _sub.CallCountA);
        }

        [Fact]
        public void Should_register_msgA_and_hold_reference() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.Equal(1, _sub.CallCountA);
        }

        [Fact]
        public void Should_register_msgA_and_msgB() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            _mediator.Publish(new MsgA());
            Assert.Equal(1, _sub.CallCountA);
            Assert.Equal(0, _sub.CallCountB);

            _mediator.Subscribe<MsgB>(_sub, _sub.Call);
            _mediator.Publish(new MsgB());
            Assert.Equal(1, _sub.CallCountA);
            Assert.Equal(1, _sub.CallCountB);
        }

        [Fact]
        public void Can_call_msgA_many_times() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            _mediator.Publish(new MsgA());
            _mediator.Publish(new MsgA());
            _mediator.Publish(new MsgA());
            Assert.Equal(3, _sub.CallCountA);
        }

        [Fact]
        public void Should_not_hold_reference() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            Assert.Equal(1, _mediator.RegisteredSubscribers());
            _sub = null;
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.Equal(0, _mediator.RegisteredSubscribers());
        }

        [Fact]
        public void Should_do_multicast() {
            var sub1 = new Sub();
            var sub2 = new Sub();
            _mediator.Subscribe<MsgA>(sub1, sub1.Call);
            _mediator.Subscribe<MsgA>(sub2, sub2.Call);
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.Equal(1, sub1.CallCountA);
            Assert.Equal(1, sub2.CallCountA);
        }

        [Fact]
        public void Should_accept_lambda_actions() {
            _mediator.Subscribe<MsgA>(_sub, m => _sub.Call(m.MsgB));
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.Equal(1, _sub.CallCountB);
        }

        [Fact]
        public void Should_accept_lambda_with_lambda_action() {
            _mediator.Subscribe<MsgA>(_sub, m => InvokeAction(() => _sub.Call(m.MsgB)));
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.Equal(1, _sub.CallCountB);
        }

        [Fact]
        public void Should_register_private_methods() {
            _sub.RegisterMsgAPrivate(_mediator);
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.Equal(1, _sub.CallCountA);
        }

        [Fact]
        public void Should_register_static_methods() {
            Sub.CallCountStatic = 0;
            _mediator.Subscribe<MsgA>(_sub, Sub.CallStatic);
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.Equal(1, Sub.CallCountStatic);
        }

        [Fact]
        public void Should_unsubscribe() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            _mediator.Publish(new MsgA());
            Assert.Equal(1, _sub.CallCountA);
            _mediator.Unsubscribe<MsgA>(_sub);
            _mediator.Publish(new MsgA());
            Assert.Equal(1, _sub.CallCountA);
        }

        private static void RunGC() {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        private void InvokeAction(Action action) {
            action.Invoke();
        }
    }

    public class MsgA {
        public MsgB MsgB { get; set; }
    }
    public class MsgB { }

    public class Sub {
        public int CallCountA { get; set; }
        public int CallCountB { get; set; }
        public static int CallCountStatic;

        public void Call(MsgA msg) {
            CallCountA++;
        }
        public void Call(MsgB msg) {
            CallCountB++;
        }

        public static void CallStatic(MsgA msg) {
            CallCountStatic++;
        }

        public void RegisterMsgAPrivate(Mediator mediator) {
            mediator.Subscribe<MsgA>(this, CallPrivate);
        }

        private void CallPrivate(MsgA msg) {
            CallCountA++;
        }
    }
}