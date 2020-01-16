#region Usings
using System.Threading;
#endregion

namespace Shin.Framework.Messaging
{
    public interface IMessage
    {
        #region Properties
        SynchronizationContext Context { get; set; }
        #endregion

        #region Methods
        void Unsubscribe(SubscriptionToken token);

        void Publish(params object[] arguments);

        bool Contains(SubscriptionToken token);
        #endregion
    }
}