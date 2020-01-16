#region Usings
#endregion

#region Usings
using System;
using Shin.Framework.Extensions;
#endregion

namespace Shin.Framework
{
    public abstract class Initializable : Disposable, IInitialize
    {
        #region Events
        public event EventHandler Initializing;
        public event EventHandler Initialized;
        #endregion

        #region Members
        protected bool m_isInitialized;
        #endregion

        #region Properties
        public bool IsInitialized
        {
            get { return m_isInitialized; }
        }
        #endregion

        protected Initializable()
        {
            WireUpInitializeEvents();
        }

        #region Methods
        protected virtual void OnInitializing(object sender, EventArgs e) { }

        protected virtual void OnInitialized(object sender, EventArgs e) { }

        protected virtual void InitializeResources() { }

        protected override void DisposeManagedResources()
        {
            Initializing.Dispose();
            Initialized.Dispose();
            base.DisposeManagedResources();
        }

        public void Initialize()
        {
            if (m_isInitialized)
                return;

            Initializing.Raise(this, EventArgs.Empty);
            InitializeResources();
            Initialized.Raise(this, EventArgs.Empty);
            m_isInitialized = true;
        }

        private void WireUpInitializeEvents()
        {
            Initializing += OnInitializing;
            Initialized += OnInitialized;
        }
        #endregion
    }
}