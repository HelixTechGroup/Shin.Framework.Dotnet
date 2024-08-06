#region Usings
using System;
using Shin.Messaging.Exceptions;
#endregion

namespace Shin.Messaging
{
    public interface IMessageProxy : IInitialize, IDispose
    {
        #region Events
        event EventHandler<MessageErrorEventArgs> OnError;
        #endregion

        #region Methods
        void RegisterGlobalHandler<TMessage>(Action<TMessage, object> onMessage) where TMessage : IMessage, new();
        #endregion
    }

    public interface IPubSubMessageProxy : IMessageProxy, IPublishMessage, ISubscribeMessage { }
}