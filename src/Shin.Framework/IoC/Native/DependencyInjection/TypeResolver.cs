#region Usings
#endregion

using System;
using System.Threading;
using Shin.Framework.Extensions;
using Shin.Framework.Threading;

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    internal sealed class TypeResolver : Disposable, IResolver
    {
        #region Members
        private readonly Func<object[], object> m_createInstanceFunc;
        private object m_instance;
        private int m_instantCount;
        private readonly bool m_singleton;
        private readonly ReaderWriterLockSlim m_lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <inheritdoc />
        private bool m_hasInstance;

        private readonly Type m_type;
        #endregion

        #region Properties
        /// <inheritdoc />
        //public Func<object[], object> CreateInstanceFunc
        //{
        //    get { return m_createInstanceFunc; }
        //}

        public bool Singleton 
        { 
            get { return m_singleton; }
        }

        /// <inheritdoc />
        public bool HasInstance
        {
            get { return m_hasInstance; }
        }

        /// <inheritdoc />
        public Type Type
        {
            get { return m_type; }
        }
        #endregion

        public TypeResolver() { }

        public TypeResolver(Type type, Func<object[], object> instanceFunc, bool singleton)
        {
            m_type = type;
            m_createInstanceFunc = instanceFunc;
            m_singleton = singleton;
        }

        #region Methods
        public object CreateObject(params object[] parameters)
        {
            m_lockSlim.TryEnter(SynchronizationAccess.Write);
            try
            {
                var result = m_createInstanceFunc?.Invoke(parameters);
                
                if (result != null)
                {
                    m_instantCount++;
                }
                return result;
            }
            finally
            {
                m_lockSlim.TryExit(SynchronizationAccess.Write);
            }
        }

        public object GetObject(params object[] parameters)
        {
            m_lockSlim.TryEnter(SynchronizationAccess.Write);
            try
            {
                if (m_singleton)
                {
                    if (m_hasInstance)
                        return m_instance;
                }

                m_instance = CreateObject(parameters);
                m_hasInstance = true;

                return m_instance;
            }
            finally
            {
                m_lockSlim.TryExit(SynchronizationAccess.Write);
            }
        }

        protected override void DisposeManagedResources()
        {
            m_instance = null;
        }
        #endregion
    }
}