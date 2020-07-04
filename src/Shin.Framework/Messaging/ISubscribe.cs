#region Usings
using System;
using System.Threading;
#endregion

namespace Shin.Framework.Messaging
{
    public interface ISubscribe
    {
        #region Properties
        SynchronizationContext Context { get; set; }

        int SubscriptionCount { get; }
        #endregion

        #region Methods
        void Unsubscribe(SubscriptionToken token);

        void ClearSubscriptions();

        bool ContainsSubscription(SubscriptionToken token);
        #endregion
    }

    public interface ISubscribe<in TSubscriber> : ISubscribe where TSubscriber : Delegate
    {
        #region Methods
        SubscriptionToken Subscribe(TSubscriber subscriber);

        SubscriptionToken Subscribe(TSubscriber subscriber, ThreadOption threadOption);

        SubscriptionToken Subscribe(TSubscriber subscriber, bool keepSubscriberReferenceAlive);

        SubscriptionToken Subscribe(TSubscriber subscriber, ThreadOption threadOption, bool keepSubscriberReferenceAlive);

        void Unsubscribe(TSubscriber subscriber);

        bool ContainsSubscription(TSubscriber subscriber);
        #endregion
    }
}