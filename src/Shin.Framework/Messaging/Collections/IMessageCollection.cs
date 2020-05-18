#region Usings
#endregion

#region Usings
#endregion

namespace Shin.Framework.Messaging.Collections
{
    public interface IMessageCollection
    {
        int Count { get; }

        #region Methods
        bool ContainsMessage<TMessage>() where TMessage : IMessage, new();

        TMessage GetMessage<TMessage>() where TMessage : IMessage, new();

        void RemoveMessage<TMessage>() where TMessage : IMessage, new();
        #endregion
    }
}