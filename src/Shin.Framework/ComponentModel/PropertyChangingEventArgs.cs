#region Usings
using System;
#endregion

namespace Shin.Framework.ComponentModel
{
    public class PropertyChangingEventArgs<T> : EventArgs
    {
        #region Members
        private readonly T m_currentValue;
        private readonly T m_requestedValue;
        #endregion

        #region Properties
        public T CurrentValue
        {
            get { return m_currentValue; }
        }

        public T RequestedValue
        {
            get { return m_requestedValue; }
        }
        #endregion

        public PropertyChangingEventArgs(T currentValue, T requestedValue)
        {
            m_currentValue = currentValue;
            m_requestedValue = requestedValue;
        }
    }
}