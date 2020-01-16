#region Usings
#endregion

#region Usings
using System;
using System.Threading;
#endregion

namespace Shin.Framework.Messaging.Subscriptions
{
    internal sealed class ContextDispatcherSubscription : Subscription
    {
        #region Members
        private readonly SynchronizationContext m_context;
        #endregion

        public ContextDispatcherSubscription(IDelegateReference actionReference,
                                             SynchronizationContext context)
            : base(actionReference)
        {
            m_context = context;
        }

        #region Methods
        protected override void InvokeAction(Action action)
        {
            //action.OnNewThread(m_context);
            //m_context.Post((o) => action(), null);
        }
        #endregion
    }

    internal sealed class ContextDispatcherSubscription<TPayload> : Subscription<TPayload>
    {
        #region Members
        private readonly SynchronizationContext m_context;
        #endregion

        public ContextDispatcherSubscription(IDelegateReference actionReference,
                                             IDelegateReference filterReference,
                                             SynchronizationContext context) :
            base(actionReference, filterReference)
        {
            m_context = context;
        }

        #region Methods
        protected override void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            //action.OnNewThread(m_context, argument);
            //m_context.Post((o) => action((TPayload)o), argument);
        }
        #endregion
    }
}