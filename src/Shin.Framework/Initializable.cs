﻿#region Usings
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
        public event EventHandler Initialized;
        public event EventHandler Initializing;
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
        public void Initialize()
        {
            if (m_isInitialized)
                return;

            lock(m_lock)
            {
                m_isDisposed = false;
                Initializing.Raise(this, EventArgs.Empty);
                InitializeResources();
                m_isInitialized = true;
                Initialized.Raise(this, EventArgs.Empty);
            }
        }

        protected virtual void InitializeResources() { }

        protected virtual void OnInitialized(object sender, EventArgs e) { }

        protected virtual void OnInitializing(object sender, EventArgs e) { }

        protected override void DisposeManagedResources()
        {
            Initializing.Dispose();
            Initialized.Dispose();
            m_isInitialized = false;
            base.DisposeManagedResources();
        }

        private void WireUpInitializeEvents()
        {
            Initializing += OnInitializing;
            Initialized += OnInitialized;
        }
        #endregion
    }
}