#region Usings
using System;
using System.Collections.Concurrent;
using Shin.Framework;
using Shin.Framework.Extensions;
#endregion

namespace Shield.Framework.IoC.Native.DependencyInjection
{
    internal sealed class ResolverDictionary : ConcurrentDictionary<string, IResolver>, IDispose
    {
        #region Events
        /// <inheritdoc />
        public event EventHandler Disposed;

        /// <inheritdoc />
        public event EventHandler Disposing;
        #endregion

        #region Members
        private bool m_isDisposed;
        #endregion

        #region Properties
        public bool IsDisposed
        {
            get { return m_isDisposed; }
        }
        #endregion

        public ResolverDictionary()
        {
            WireUpDisposeEvents();
        }

        ~ResolverDictionary()
        {
            Dispose(false);
        }

        #region Methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void DisposeManagedResources()
        {
            foreach (var value in Values)
                value.Dispose();
        }

        private void DisposeUnmanagedResources() { }

        private void OnDisposing(object sender, EventArgs e) { }

        private void OnDisposed(object sender, EventArgs e) { }

        private void Dispose(bool disposing)
        {
            if (m_isDisposed)
                return;

            Disposing.Raise(this, EventArgs.Empty);
            if (disposing)
                DisposeManagedResources();

            DisposeUnmanagedResources();
            Disposed.Raise(this, EventArgs.Empty);

            Disposing.Dispose();
            Disposed.Dispose();
            m_isDisposed = true;
        }

        private void WireUpDisposeEvents()
        {
            Disposing += OnDisposing;
            Disposed += OnDisposed;
        }
        #endregion
    }
}