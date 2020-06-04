using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Shin.Framework.Messaging
{
    public interface ISubscribe
    {
        SynchronizationContext Context { get; set; }

        int SubscriptionCount { get; }

        void Unsubscribe(SubscriptionToken token);

        void ClearSubscriptions();

        bool ContainsSubscription(SubscriptionToken token);
    }

    public interface ISubscribe<in TSubscriber> : ISubscribe where TSubscriber : Delegate
    {
        SubscriptionToken Subscribe(TSubscriber subscriber);

        SubscriptionToken Subscribe(TSubscriber subscriber, ThreadOption threadOption);

        SubscriptionToken Subscribe(TSubscriber subscriber, bool keepSubscriberReferenceAlive);

        SubscriptionToken Subscribe(TSubscriber subscriber, ThreadOption threadOption, bool keepSubscriberReferenceAlive);

        void Unsubscribe(TSubscriber subscriber);

        bool ContainsSubscription(TSubscriber subscriber);
    }
}
