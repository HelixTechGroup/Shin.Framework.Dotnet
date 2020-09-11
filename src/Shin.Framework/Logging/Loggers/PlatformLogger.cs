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
        #endregion

        #region Methods
        public abstract void Flush(ILogEntry entry);

        protected abstract bool ShouldBuffer();
        #endregion
    }
}