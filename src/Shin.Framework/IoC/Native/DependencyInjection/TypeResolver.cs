#region Usings
using System;
using Shin.Framework;
#endregion

namespace Shield.Framework.IoC.Native.DependencyInjection
{
    internal sealed class TypeResolver : Disposable, IResolver
    {
        #region Members
        private Func<object> m_createInstanceFunc;
        private object m_instance;
        #endregion

        #region Properties
        /// <inheritdoc />
        public Func<object> CreateInstanceFunc
        {
            get { return m_createInstanceFunc; }
            set { m_createInstanceFunc = value; }
        }

        public bool Singleton { get; set; }
        #endregion

        #region Methods
        protected override void DisposeManagedResources()
        {
            m_instance = null;
            CreateInstanceFunc = null;
        }

        public object GetObject()
        {
            if (!Singleton)
                return m_createInstanceFunc();

            if (m_instance != null)
                return m_instance;

            m_instance = m_createInstanceFunc();

            //if (m_instance != null)
            //    CreateInstanceFunc = null;

            return m_instance;
        }
        #endregion
    }
}