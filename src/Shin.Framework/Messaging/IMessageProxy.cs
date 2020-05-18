using System;
using System.Threading;
using Shin.Framework.Messaging.Exceptions;

namespace Shin.Framework.Messaging
{
    public interface IMessageProxy : IInitialize, IDispose
    {
        event EventHandler<MessageErrorEventArgs> OnError;

        void RegisterGlobalHandler<TMessage>(Action<TMessage, object> onMessage) where TMessage : IMessage, new();
    }

    public interface IPubSubMessageProxy : IMessageProxy, IPublishMessage, ISubscribeMessage { }
}
