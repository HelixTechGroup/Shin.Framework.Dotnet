#region Usings
using System;
using Shin.Framework;
#endregion

namespace Shield.Framework.IoC.Native.DependencyInjection
{
    internal sealed class TypeResolver : Disposable, IResolver
    {
        #region Members
        private Func<object[], object> m_createInstanceFunc;
        private object m_instance;
        private int m_instantCount;

        /// <inheritdoc />
        private bool m_hasInstance;

        private Type m_type;
        #endregion

        #region Properties
        /// <inheritdoc />
        public Func<object[], object> CreateInstanceFunc
        {
            get { return m_createInstanceFunc; }
            set { m_createInstanceFunc = value; }
        }

        public bool Singleton { get; set; }

        /// <inheritdoc />
        public bool HasInstance
        {
            get { return m_hasInstance; }
        }

        /// <inheritdoc />
        public Type Type { get; set; }
        #endregion

        #region Methods
        public object GetObject(params object[] parameters)
        {
            if (!Singleton)
            {
                m_instantCount++;
                return m_createInstanceFunc(parameters);
            }

            if (m_hasInstance)
                return m_instance;

            m_instantCount++;
            m_instance = m_createInstanceFunc(parameters);

            //if (m_instance != null)
            //    CreateInstanceFunc = null;
            m_hasInstance = true;
            return m_instance;
        }

        protected override void DisposeManagedResources()
        {
            m_instance = null;
            CreateInstanceFunc = null;
        }
        #endregion
    }
}