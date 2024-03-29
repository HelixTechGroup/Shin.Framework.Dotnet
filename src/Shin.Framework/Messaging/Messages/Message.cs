﻿#region Usings
#endregion

#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Shin.Framework.Collections.Concurrent;
#endregion

namespace Shin.Framework.Messaging.Messages
{
    public abstract class Message : IMessage, IEquatable<Message>
    {
        #region Members
        internal readonly ConcurrentList<ISubscription> m_subscriptions;
        protected SynchronizationContext m_context;
        private readonly DateTime m_dateCreated;
        #endregion

        #region Properties
        /// <inheritdoc />
        public SynchronizationContext Context
        {
            get { return m_context; }
            set { m_context = value; }
        }

        /// <inheritdoc />
        public int SubscriptionCount
        {
            get { return m_subscriptions.Count; }
        }
        #endregion

        protected Message()
        {
            m_dateCreated = DateTime.UtcNow.Date;
            m_subscriptions = new ConcurrentList<ISubscription>();
        }

        #region Methods
        public virtual void Unsubscribe(SubscriptionToken token)
        {
            var subscription = m_subscriptions.FirstOrDefault(evt => evt.Token == token);
            if (subscription != null)
                m_subscriptions.Remove(subscription);
        }

        public virtual void Publish(params object[] arguments)
        {
            var executionStrategies = PruneAndReturnStrategies();
            foreach (var executionStrategy in executionStrategies)
                executionStrategy(arguments);
        }

        public virtual void Publish()
        {
            Publish(new object[] { });
        }

        public bool ContainsSubscription(SubscriptionToken token)
        {
            var subscription = m_subscriptions.FirstOrDefault(evt => evt.Token == token);
            return subscription != null;
        }

        /// <inheritdoc />
        public void ClearSubscriptions()
        {
            m_subscriptions.Clear();
        }

        internal SubscriptionToken Subscribe(ISubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            subscription.Token = new SubscriptionToken(Unsubscribe);
            m_subscriptions.Add(subscription);

            return subscription.Token;
        }

        protected IEnumerable<Action<object[]>> PruneAndReturnStrategies()
        {
            var returnList = new ConcurrentList<Action<object[]>>();
            for (var i = m_subscriptions.Count - 1; i >= 0; i--)
            {
                var listItem = m_subscriptions[i].GetExecutionStrategy();
                if (listItem == null)
                    m_subscriptions.RemoveAt(i);
                else
                    returnList.Add(listItem);
            }

            return returnList;
        }
        #endregion

        /// <inheritdoc />
        public bool Equals(Message other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return true;
        }

        /// <inheritdoc />
        public bool Equals(IMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is Message other && Equals(other);
        }


        public static bool operator ==(Message left, Message right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Message left, Message right)
        {
            return !Equals(left, right);
        }
    }
}