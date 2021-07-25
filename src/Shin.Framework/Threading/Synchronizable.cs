using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Threading
{
    public abstract class Synchronizable : Disposable, ISynchronize
    {
        /// <inheritdoc />
        public void Enter()
        {
            if (!TryEnter())
                throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public void Exit()
        {
            if (!TryExit())
                throw new InvalidOperationException();
        }

        protected abstract bool TryEnter(SynchronizationAccess access = SynchronizationAccess.Read,
                                         int maxRetries = 3,
                                         int retryDelay = 50,
                                         int lockTimeout = 50);

        protected abstract bool TryExit(SynchronizationAccess access = SynchronizationAccess.Read,
                                        int maxRetries = 3,
                                        int retryDelay = 50,
                                        int lockTimeout = 50);
    }

    public abstract class Synchronizable<TObject> : Disposable, ISynchronize<TObject>
    {
        protected readonly TObject m_rawObject;

        Synchronizable(TObject rawObject)
        {
            m_rawObject = rawObject;
        }

        /// <inheritdoc />
        public TObject Enter()
        {
            if (TryEnter())
                return m_rawObject;

            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public void Exit()
        {
            if (!TryExit())
                throw new InvalidOperationException();
        }

        protected abstract bool TryEnter(SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50);

        protected abstract bool TryExit(SynchronizationAccess access = SynchronizationAccess.Read, int maxRetries = 3, int retryDelay = 50, int lockTimeout = 50);
    }
}
