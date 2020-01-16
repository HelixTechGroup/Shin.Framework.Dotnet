#region Usings
#endregion

#region Usings
using System;
using System.Collections.Concurrent;
using System.Threading;
#endregion

namespace Shin.Framework.Messaging
{
    public sealed class MessageAggregator : Initializable, IMessageAggregator
    {
        #region Members
        private readonly SynchronizationContext m_context;
        private readonly ConcurrentDictionary<Type, IMessage> m_messages;
        #endregion

        public MessageAggregator()
        {
            m_context = SynchronizationContext.Current;
            m_messages = new ConcurrentDictionary<Type, IMessage>();
        }

        #region Methods
        public bool MessageExists<T>() where T : IMessage, new()
        {
            return m_messages.ContainsKey(typeof(T));
        }

        public T GetMessage<T>() where T : IMessage, new()
        {
            if (m_messages.TryGetValue(typeof(T), out var message))
                return (T)message;

            var newMessage = new T();
            newMessage.Context = m_context;
            m_messages[typeof(T)] = newMessage;

            return newMessage;
        }

        public void RemoveMessage<T>() where T : IMessage, new()
        {
            if (!MessageExists<T>())
                return;

            m_messages.TryRemove(typeof(T), out var message);
        }
        #endregion
    }
}