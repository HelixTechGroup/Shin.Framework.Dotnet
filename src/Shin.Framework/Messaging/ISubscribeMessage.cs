using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Messaging
{
    public interface ISubscribeMessage
    {
        SubscriptionToken Subscribe<TMessage, TSubscriber>(TSubscriber subscriber) where TMessage : IMessage, new() where TSubscriber : Delegate;

        SubscriptionToken Subscribe<TMessage, TSubscriber>(TSubscriber subscriber, ThreadOption threadOption)
            where TMessage : IMessage, new() where TSubscriber : Delegate;

        SubscriptionToken Subscribe<TMessage, TSubscriber>(TSubscriber subscriber, bool keepSubscriberReferenceAlive)
            where TMessage : IMessage, new() where TSubscriber : Delegate;

        SubscriptionToken Subscribe<TMessage, TSubscriber>(TSubscriber subscriber, ThreadOption threadOption, bool keepSubscriberReferenceAlive)
            where TMessage : IMessage, new() where TSubscriber : Delegate;

        void Unsubscribe(SubscriptionToken token);

        void Unsubscribe<TMessage, TSubscriber>(TSubscriber subscriber) where TMessage : IMessage, new() where TSubscriber : Delegate;

        void ClearSubscriptions<TMessage>() where TMessage : IMessage, new();

        void ClearSubscriptions();

        bool ContainsSubscription(SubscriptionToken token);

        bool ContainsSubscription<TMessage, TSubscriber>(TSubscriber subscriber) where TMessage : IMessage, new() where TSubscriber : Delegate;
    }
}
