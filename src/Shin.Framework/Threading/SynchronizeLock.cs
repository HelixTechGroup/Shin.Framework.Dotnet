using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Threading
{
    public abstract class SynchronizableLock: Synchronizable, ISynchronizeLock
    {
        private bool m_isLocked;

        /// <inheritdoc />
        public virtual bool IsLocked
        {
            get { return m_isLocked; }
            set { m_isLocked = value; }
        }
    }
}
