#region Usings
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public abstract class PlatformLogger : Disposable, ILogProvider
    {
        #region Members
        protected bool m_isBuffering = false;
        #endregion

        #region Properties
        /// <inheritdoc />
        public bool IsBuffering
        {
            get { return m_isBuffering; }
        }

        /// <inheritdoc />
        public void Flush(ILogEntry entry)
        {
            lock(m_lock)
            {
                PlatformFlush(entry);
            }
        }
        #endregion

        #region Methods
        protected abstract void PlatformFlush(ILogEntry entry);

        protected abstract bool ShouldBuffer();
        #endregion
    }
}