#region Usings
using System;
using System.Runtime.ConstrainedExecution;
using Shin.Framework.Extensions;
#endregion

namespace Shin.Framework
{
    public abstract class Disposable : IDispose
    {
        #region Events
        /// <inheritdoc />
        public event EventHandler Disposed;

        /// <inheritdoc />
        public event EventHandler Disposing;
        #endregion

        #region Members
        protected bool m_isDisposed;
        protected static readonly object m_lock = new object();
        #endregion

        #region Properties
        /// <inheritdoc />
        public bool IsDisposed
        {
            get { return m_isDisposed; }
        }
        #endregion

        protected Disposable()
        {
            m_isDisposed = false;
            WireUpDisposeEvents();
        }

        ~Disposable()
        {
            Dispose(false);
        }

        #region Methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeManagedResources() { }

        protected virtual void DisposeUnmanagedResources() { }

        protected virtual void OnDisposed(object sender, EventArgs e) { }

        protected virtual void OnDisposing(object sender, EventArgs e) { }

        protected void Dispose(bool disposing)
        {
            if (m_isDisposed)
                return;

            lock(m_lock)
            {
                Disposing.Raise(this, EventArgs.Empty);
                if (disposing)
                    DisposeManagedResources();

                DisposeUnmanagedResources();
                Disposed.Raise(this, EventArgs.Empty);

                Disposing.Dispose();
                Disposed.Dispose();
                m_isDisposed = true;
            }
        }

        private void WireUpDisposeEvents()
        {
            Disposing += OnDisposing;
            Disposed += OnDisposed;
        }
        #endregion
    }
}