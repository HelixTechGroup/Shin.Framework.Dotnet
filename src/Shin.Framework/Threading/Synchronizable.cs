#region Usings
using System;
#endregion

namespace Shin.Framework.Threading
{
    public abstract class Synchronizable : Disposable,
                                                               ISynchronize
    {
#region Methods
        /// <inheritdoc />
        public void Enter()
        {
            if (!TryEnterLock()) throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public void Exit()
        {
            if (!TryExit()) throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public bool TryEnter()
        {
            return TryEnterLock();
        }

        /// <inheritdoc />
        public bool TryExit()
        {
            return TryExitLock();
        }

        protected abstract bool TryEnterLock(
            SynchronizationAccess access = SynchronizationAccess.Read,
                                         int maxRetries = 3,
                                         int retryDelay = 50,
                                         int lockTimeout = 50);

        protected abstract bool TryExitLock(
            SynchronizationAccess access = SynchronizationAccess.Read,
                                        int maxRetries = 3,
                                        int retryDelay = 50,
                                        int lockTimeout = 50);
#endregion
    }

//    public abstract class Synchronizable<TContext> : Disposable,
//                                                     ISynchronizeObject<TContext>
//    {
//#region Members
//        protected readonly TContext m_rawObject;
//#endregion

//        Synchronizable(TContext rawObject)
//        {
//            m_rawObject = rawObject;
//        }

//#region Methods
//        /// <inheritdoc />
//        public TContext Enter()
//        {
//            if (TryEnter()) return m_rawObject;

//            throw new InvalidOperationException();
//        }

//        /// <inheritdoc />
//        public void Exit()
//        {
//            if (!TryExit()) throw new InvalidOperationException();
//        }

//        /// <inheritdoc />
//        void ISynchronize.Enter()
//        {
//            throw new NotImplementedException();
//        }

//        protected abstract bool TryEnter(SynchronizationAccess access = SynchronizationAccess.Read,
//                                         int maxRetries = 3,
//                                         int retryDelay = 50,
//                                         int lockTimeout = 50);

//        protected abstract bool TryExit(SynchronizationAccess access = SynchronizationAccess.Read,
//                                        int maxRetries = 3,
//                                        int retryDelay = 50,
//                                        int lockTimeout = 50);
//#endregion
//    }
}