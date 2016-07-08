using System;
using NUnit.Framework;

namespace SharpMediator.Tests {
    public class MediatorTests {

        private Mediator _mediator;
        private Sub _sub;

        [SetUp]
        public void SetUp() {
            _mediator = new Mediator();
            _sub = new Sub();
        }
        [Test]
        public void Should_register_msgA() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, _sub.CallCountA);
        }

        [Test]
        public void Should_register_msgA_and_hold_reference() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, _sub.CallCountA);
        }

        [Test]
        public void Should_register_msgA_and_msgB() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, _sub.CallCountA);
            Assert.AreEqual(0, _sub.CallCountB);

            _mediator.Subscribe<MsgB>(_sub, _sub.Call);
            _mediator.Publish(new MsgB());
            Assert.AreEqual(1, _sub.CallCountA);
            Assert.AreEqual(1, _sub.CallCountB);
        }

        [Test]
        public void Can_call_msgA_many_times() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            _mediator.Publish(new MsgA());
            _mediator.Publish(new MsgA());
            _mediator.Publish(new MsgA());
            Assert.AreEqual(3, _sub.CallCountA);
        }

        [Test]
        public void Should_not_hold_reference() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            Assert.AreEqual(1, _mediator.RegisteredSubscribers());
            _sub = null;
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.AreEqual(0, _mediator.RegisteredSubscribers());
        }

        [Test]
        public void Should_do_multicast() {
            var sub1 = new Sub();
            var sub2 = new Sub();
            _mediator.Subscribe<MsgA>(sub1, sub1.Call);
            _mediator.Subscribe<MsgA>(sub2, sub2.Call);
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, sub1.CallCountA);
            Assert.AreEqual(1, sub2.CallCountA);
        }

        [Test]
        public void Should_accept_lambda_actions() {
            _mediator.Subscribe<MsgA>(_sub, m => _sub.Call(m.MsgB));
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, _sub.CallCountB);
        }

        [Test]
        public void Should_accept_lambda_with_lambda_action() {
            _mediator.Subscribe<MsgA>(_sub, m => InvokeAction(() => _sub.Call(m.MsgB)));
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, _sub.CallCountB);
        }

        [Test]
        public void Should_register_private_methods() {
            _sub.RegisterMsgAPrivate(_mediator);
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, _sub.CallCountA);
        }

        [Test]
        public void Should_register_static_methods() {
            Sub.CallCountStatic = 0;
            _mediator.Subscribe<MsgA>(_sub, Sub.CallStatic);
            RunGC();
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, Sub.CallCountStatic);
        }

        [Test]
        public void Should_unsubscribe() {
            _mediator.Subscribe<MsgA>(_sub, _sub.Call);
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, _sub.CallCountA);
            _mediator.Unsubscribe<MsgA>(_sub);
            _mediator.Publish(new MsgA());
            Assert.AreEqual(1, _sub.CallCountA);
        }

        private static void RunGC() {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
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