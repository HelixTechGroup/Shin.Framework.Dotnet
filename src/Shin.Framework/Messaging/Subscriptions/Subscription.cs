#region Usings
using System;
using System.Globalization;
#endregion

namespace Shin.Framework.Messaging.Subscriptions
{
    internal class Subscription : ISubscription
    {
        #region Members
        private readonly IDelegateReference m_actionReference;
        private SubscriptionToken m_subscriptionToken;
        #endregion

        #region Properties
        public Delegate Action
        {
            get { return m_actionReference.Target; }
        }

        public SubscriptionToken Token
        {
            get { return m_subscriptionToken; }
            set { m_subscriptionToken = value; }
        }
        #endregion

        public Subscription(IDelegateReference actionReference)
        {
            if (actionReference == null)
                throw new ArgumentNullException(nameof(actionReference));
            if (!(actionReference.Target is Action))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                          "Invalid Delegate Reference Type Exception",
                                                          typeof(Action).FullName),
                                            nameof(actionReference));

            m_actionReference = actionReference;
        }

        #region Methods
        protected virtual void InvokeAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            action();
        }

        public Action<object[]> GetExecutionStrategy()
        {
            if (m_actionReference.Target is Action action)
                return arguments => InvokeAction(action);

            return null;
        }
        #endregion
    }

    internal class Subscription<TPayload> : ISubscription
    {
        #region Members
        private readonly IDelegateReference m_actionReference;
        private readonly IDelegateReference m_filterReference;
        private SubscriptionToken m_token;
        #endregion

        #region Properties
        public Delegate Action
        {
            get { return m_actionReference.Target; }
        }

        public SubscriptionToken Token
        {
            get { return m_token; }
            set { m_token = value; }
        }
        #endregion

        public Subscription(IDelegateReference actionReference, IDelegateReference filterReference)
        {
            if (actionReference == null)
                throw new ArgumentNullException(nameof(actionReference));
            if (!(actionReference.Target is Action<TPayload>))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                          "Invalid Delegate Reference Type Exception",
                                                          typeof(Action<TPayload>).FullName),
                                            nameof(actionReference));

            if (filterReference == null)
                throw new ArgumentNullException(nameof(filterReference));
            if (!(filterReference.Target is Predicate<TPayload>))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                          "Invalid Delegate Reference Type Exception",
                                                          typeof(Predicate<TPayload>).FullName),
                                            nameof(filterReference));

            m_actionReference = actionReference;
            m_filterReference = filterReference;
        }

        #region Methods
        protected virtual void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            action(argument);
        }

        public Action<object[]> GetExecutionStrategy()
        {
            if (m_actionReference.Target is Action<TPayload> action && m_filterReference.Target is Predicate<TPayload> filter)
            {
                return arguments =>
                       {
                           TPayload argument = default;
                           if (arguments != null && arguments.Length > 0 && arguments[0] != null)
                               argument = (TPayload)arguments[0];

                           if (filter(argument))
                               InvokeAction(action, argument);
                       };
            }

            return null;
        }
        #endregion
    }
}