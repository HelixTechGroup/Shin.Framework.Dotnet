#region Usings
using System;
#endregion

namespace Shin.Messaging
{
    internal interface ISubscription
    {
        #region Properties
        Delegate Action { get; }
        SubscriptionToken Token { get; set; }
        #endregion

        #region Methods
        Action<object[]> GetExecutionStrategy();
        #endregion
    }
}