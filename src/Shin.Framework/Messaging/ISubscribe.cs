using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Messaging
{
    public interface ISubscribe<in TSubscriber> where TSubscriber : Delegate
    {
        int SubscriptionCount { get; }

        SubscriptionToken Subscribe(TSubscriber subscriber);

        SubscriptionToken Subscribe(TSubscriber subscriber, ThreadOption threadOption);

        SubscriptionToken Subscribe(TSubscriber subscriber, bool keepSubscriberReferenceAlive);

        SubscriptionToken Subscribe(TSubscriber subscriber, ThreadOption threadOption, bool keepSubscriberReferenceAlive);

        void Unsubscribe(SubscriptionToken token);

        void Unsubscribe(TSubscriber subscriber);

        void ClearSubscriptions();

        bool ContainsSubscription(SubscriptionToken token);

        bool ContainsSubscription(TSubscriber subscriber);
    }
}
