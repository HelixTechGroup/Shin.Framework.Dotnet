#region Usings
using System;
using System.Reflection;
#endregion

namespace Shin.Framework.Messaging
{
    internal sealed class DelegateReference : IDelegateReference
    {
        #region Members
        private readonly Type m_delegateType;
        private readonly MethodInfo m_method;
        private readonly Delegate m_target;
        private readonly WeakReference m_weakReference;
        #endregion

        #region Properties
        public Delegate Target
        {
            get { return m_target ?? TryGetDelegate(); }
        }
        #endregion

        public DelegateReference(Delegate @delegate, bool keepReferenceAlive)
        {
            if (@delegate == null)
                throw new ArgumentNullException(nameof(@delegate));

            if (keepReferenceAlive)
                m_target = @delegate;
            else
            {
                m_weakReference = new WeakReference(@delegate.Target);
                m_method = @delegate.GetMethodInfo();
                m_delegateType = @delegate.GetType();
            }
        }

        #region Methods
        private Delegate TryGetDelegate()
        {
            if (m_method.IsStatic)
                return m_method.CreateDelegate(m_delegateType, null);

            var target = m_weakReference.Target;
            return target != null ? m_method.CreateDelegate(m_delegateType, target) : null;
        }
        #endregion
    }
}