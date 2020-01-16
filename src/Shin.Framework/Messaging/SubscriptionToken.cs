#region Usings
#endregion

#region Usings
using System;
#endregion

namespace Shin.Framework.Messaging
{
    public sealed class SubscriptionToken : IEquatable<SubscriptionToken>, IDispose
    {
        #region Events
        public event Action<IDispose> OnDispose;

        /// <inheritdoc />
        public event EventHandler Disposing;

        /// <inheritdoc />
        public event EventHandler Disposed;
        #endregion

        #region Members
        private readonly Guid m_token;
        private bool m_isDisposed;
        private Action<SubscriptionToken> m_unsubscribeAction;
        #endregion

        #region Properties
        public bool IsDisposed
        {
            get { return m_isDisposed; }
        }
        #endregion

        public SubscriptionToken(Action<SubscriptionToken> unsubscribeAction)
        {
            m_unsubscribeAction = unsubscribeAction;
            m_token = Guid.NewGuid();
        }

        ~SubscriptionToken()
        {
            Dispose(false);
        }

        #region Methods
        public static bool operator ==(SubscriptionToken left, SubscriptionToken right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SubscriptionToken left, SubscriptionToken right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || Equals(obj as SubscriptionToken);
        }

        public override int GetHashCode()
        {
            return m_token.GetHashCode();
        }

        public bool Equals(SubscriptionToken other)
        {
            return other != null && Equals(m_token, other.m_token);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (m_isDisposed)
                return;

            if (disposing) { }

            if (m_unsubscribeAction != null)
            {
                m_unsubscribeAction(this);
                m_unsubscribeAction = null;
            }

            OnDispose?.Invoke(this);
            m_isDisposed = true;
        }
        #endregion
    }
}