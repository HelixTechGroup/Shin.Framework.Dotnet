using System;
using System.Collections.Concurrent;
using Shin.Extensions;

namespace Shin.Collections.Concurrent
{
    public abstract class DisposableConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>,
                                                             IDispose where TValue : IDisposable
    {
        private bool m_isDisposed;

        protected DisposableConcurrentDictionary()
        {
            WireUpDisposeEvents();
        }

        /// <inheritdoc />
        public event EventHandler Disposed;

        /// <inheritdoc />
        public event EventHandler Disposing;

        public bool IsDisposed
        {
            get { return m_isDisposed; }
        }

        ~DisposableConcurrentDictionary()
        {
            Dispose(false);
        }

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
            Disposed  += OnDisposed;
        }
    }
}