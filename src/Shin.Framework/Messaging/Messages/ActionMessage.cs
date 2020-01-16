#region Usings
using System;
using System.Linq;
using Shin.Framework.Messaging.Subscriptions;
#endregion

namespace Shin.Framework.Messaging.Messages
{
    public class ActionMessage : Message, IActionMessage
    {
        #region Methods
        public virtual SubscriptionToken Subscribe(Action action, ThreadOption threadOption, bool keepSubscriberReferenceAlive)
        {
            var actionReference = new DelegateReference(action, keepSubscriberReferenceAlive);

            ISubscription subscription;
            switch (threadOption)
            {
                case ThreadOption.PublisherThread:
                    subscription = new Subscription(actionReference);
                    break;
                case ThreadOption.BackgroundThread:
                    subscription = new BackgroundDispatcherSubscription(actionReference);
                    break;
                case ThreadOption.UIThread:
                    if (m_context == null)
                        throw new
                            InvalidOperationException("To use the UIThread option for subscribing, the EventAggregator must be constructed on the UI thread.");

                    subscription = new ContextDispatcherSubscription(actionReference, m_context);
                    break;
                default:
                    subscription = new Subscription(actionReference);
                    break;
            }

            return Subscribe(subscription);
        }

        public SubscriptionToken Subscribe(Action action)
        {
            return Subscribe(action, ThreadOption.PublisherThread);
        }

        public SubscriptionToken Subscribe(Action action, ThreadOption threadOption)
        {
            return Subscribe(action, threadOption, false);
        }

        public SubscriptionToken Subscribe(Action action, bool keepSubscriberReferenceAlive)
        {
            return Subscribe(action, ThreadOption.PublisherThread, keepSubscriberReferenceAlive);
        }

        public bool Contains(Action subscriber)
        {
            var eventSubscription = m_subscriptions.Cast<Subscription>().FirstOrDefault(evt => (Action)evt.Action == subscriber);
            return eventSubscription != null;
        }
        #endregion
    }

    public class ActionMessage<TPayload> : Message, IActionMessage<TPayload>
    {
        #region Methods
        public virtual SubscriptionToken Subscribe(Action<TPayload> action,
                                                   ThreadOption threadOption,
                                                   bool keepSubscriberReferenceAlive,
                                                   Predicate<TPayload> filter)
        {
            var actionReference = new DelegateReference(action, keepSubscriberReferenceAlive);

            var filterReference = new DelegateReference(new Predicate<TPayload>(delegate { return true; }), true);
            if (filter != null)
                filterReference = new DelegateReference(filter, keepSubscriberReferenceAlive);

            ISubscription subscription;
            switch (threadOption)
            {
                case ThreadOption.PublisherThread:
                    subscription = new Subscription<TPayload>(actionReference, filterReference);
                    break;
                case ThreadOption.BackgroundThread:
                    subscription = new BackgroundDispatcherSubscription<TPayload>(actionReference, filterReference);
                    break;
                case ThreadOption.UIThread:
                    if (m_context == null)
                        throw new
                            InvalidOperationException("To use the UIThread option for subscribing, the EventAggregator must be constructed on the UI thread.");

                    subscription = new ContextDispatcherSubscription<TPayload>(actionReference, filterReference, m_context);
                    break;
                default:
                    subscription = new Subscription<TPayload>(actionReference, filterReference);
                    break;
            }

            return Subscribe(subscription);
        }

        public SubscriptionToken Subscribe(Action<TPayload> action)
        {
            return Subscribe(action, ThreadOption.PublisherThread);
        }

        public SubscriptionToken Subscribe(Action<TPayload> action, ThreadOption threadOption)
        {
            return Subscribe(action, threadOption, false);
        }

        public SubscriptionToken Subscribe(Action<TPayload> action, bool keepSubscriberReferenceAlive)
        {
            return Subscribe(action, ThreadOption.PublisherThread, keepSubscriberReferenceAlive);
        }

        public SubscriptionToken Subscribe(Action<TPayload> action, ThreadOption threadOption, bool keepSubscriberReferenceAlive)
        {
            return Subscribe(action, threadOption, keepSubscriberReferenceAlive, null);
        }

        public void Publish(TPayload payload)
        {
            Publish(new object[] {payload});
        }

        public bool Contains(Action<TPayload> subscriber)
        {
            var eventSubscription = m_subscriptions.Cast<Subscription>().FirstOrDefault(evt => (Action<TPayload>)evt.Action == subscriber);
            return eventSubscription != null;
        }
        #endregion
    }
}