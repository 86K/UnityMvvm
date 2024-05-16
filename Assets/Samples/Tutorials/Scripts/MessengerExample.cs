

using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public class TestMessage : MessageBase
    {
        private readonly string content;

        public TestMessage(object sender, string content) : base(sender)
        {
            this.content = content;
        }

        public string Content => content;
    }

    public class MessengerExample : MonoBehaviour
    {
        private IMessenger messenger;
        private ISubscription<TestMessage> subscription;
        private ISubscription<TestMessage> subscriptionInUIsThread;

        public IMessenger Messenger => messenger;

        void Start()
        {
            messenger = new Messenger();

            
            subscription = messenger.Subscribe<TestMessage>(OnMessage);

            //Use the ObserveOn() method to change the message consumption thread to the UI thread.
            subscriptionInUIsThread = messenger.Subscribe<TestMessage>(OnMessageInUIThread).ObserveOn(SynchronizationContext.Current);

            

            
#if UNITY_WEBGL && !UNITY_EDITOR
            this.messenger.Publish(new TestMessage(this, "This is a test."));
#else
            Task.Run(() =>
            {
                messenger.Publish(new TestMessage(this, "This is a test."));
            });
#endif
        }

        protected void OnMessage(TestMessage message)
        {
            Debug.LogFormat("ThreadID:{0} Received:{1}", Thread.CurrentThread.ManagedThreadId, message.Content);
        }

        protected void OnMessageInUIThread(TestMessage message)
        {
            Debug.LogFormat("ThreadID:{0} Received:{1}", Thread.CurrentThread.ManagedThreadId, message.Content);
        }

        void OnDestroy()
        {
            if (subscription != null)
            {
                subscription.Dispose();
                subscription = null;
            }

            if (subscriptionInUIsThread != null)
            {
                subscriptionInUIsThread.Dispose();
                subscriptionInUIsThread = null;
            }
        }

    }
}