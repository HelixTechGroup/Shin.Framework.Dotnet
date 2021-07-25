using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Threading
{
    public abstract class SynchronizableLock: Synchronizable, ISynchronizeLock
    {
        private ISynchronizeContext m_context;

        /// <inheritdoc />
        public ISynchronizeContext Context
        {
            get { return m_context; }
        }
    }
}
