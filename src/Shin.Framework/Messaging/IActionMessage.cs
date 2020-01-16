#region Usings
using System;
#endregion

namespace Shin.Framework.Messaging
{
    public interface IActionMessage : IMessage
    {
        #region Methods
        SubscriptionToken Subscribe(Action action);

        SubscriptionToken Subscribe(Action action, ThreadOption threadOption);

        SubscriptionToken Subscribe(Action action, bool keepSubscriberReferenceAlive);

        SubscriptionToken Subscribe(Action action, ThreadOption threadOption, bool keepSubscriberReferenceAlive);

        bool Contains(Action subscriber);
        #endregion
    }

    public interface IActionMessage<TPayload> : IMessage
    {
        #region Methods
        SubscriptionToken Subscribe(Action<TPayload> action);

        SubscriptionToken Subscribe(Action<TPayload> action, ThreadOption threadOption);

        SubscriptionToken Subscribe(Action<TPayload> action, bool keepSubscriberReferenceAlive);

        SubscriptionToken Subscribe(Action<TPayload> action, ThreadOption threadOption, bool keepSubscriberReferenceAlive);

        void Publish(TPayload payload);

        bool Contains(Action<TPayload> subscriber);
        #endregion
    }
}