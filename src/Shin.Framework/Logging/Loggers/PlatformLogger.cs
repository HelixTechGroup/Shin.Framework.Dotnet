#region Usings
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public abstract class PlatformLogger : Disposable, ILogProvider
    {
        protected bool m_isBuffering;

        #region Methods
        /// <inheritdoc />
        public bool IsBuffering
        {
            get { return m_isBuffering; }
        }

        public abstract void Flush(ILogEntry entry);

        protected abstract bool ShouldBuffer();
        #endregion
    }
}