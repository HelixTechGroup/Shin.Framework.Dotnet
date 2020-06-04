#region Usings
#endregion

#region Usings
using System;
using System.Collections.Concurrent;
using System.Threading;
#endregion

namespace Shin.Framework.Messaging.Collections
{
    public sealed class MessageCollection : IMessageCollection
    {
        #region Members
        private readonly SynchronizationContext m_context;
        private readonly ConcurrentDictionary<Type, IMessage> m_messages;
        #endregion

        public MessageCollection() : this(SynchronizationContext.Current) { }

        public MessageCollection(SynchronizationContext context)
        {
            m_context = context;
            m_messages = new ConcurrentDictionary<Type, IMessage>();
        }

        #region Methods
        /// <inheritdoc />
        public int Count
        {
            get { return m_messages.Count; }
        }

        public bool ContainsMessage<T>() where T : IMessage, new()
        {
            return m_messages.ContainsKey(typeof(T));
        }

        public T GetMessage<T>() where T : IMessage, new()
        {
            if (m_messages.TryGetValue(typeof(T), out var message))
                return (T)message;

            var newMessage = new T();
            (newMessage as ISubscribe).Context = m_context;
            m_messages[typeof(T)] = newMessage;

            return newMessage;
        }

        public void RemoveMessage<T>() where T : IMessage, new()
        {
            if (!ContainsMessage<T>())
                return;

            m_messages.TryRemove(typeof(T), out var message);
        }
        #endregion
    }
}