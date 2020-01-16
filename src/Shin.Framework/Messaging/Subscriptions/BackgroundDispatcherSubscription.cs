#region Usings
#endregion

#region Usings
using System;
#endregion

namespace Shin.Framework.Messaging.Subscriptions
{
    internal sealed class BackgroundDispatcherSubscription : Subscription
    {
        public BackgroundDispatcherSubscription(IDelegateReference actionReference) :
            base(actionReference) { }

        #region Methods
        protected override void InvokeAction(Action action)
        {
            //action.OnNewThread();
        }
        #endregion
    }

    internal sealed class BackgroundDispatcherSubscription<TPayload> : Subscription<TPayload>
    {
        public BackgroundDispatcherSubscription(IDelegateReference actionReference,
                                                IDelegateReference filterReference) :
            base(actionReference, filterReference) { }

        #region Methods
        protected override void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            //action.OnNewThread(argument);
        }
        #endregion
    }
}