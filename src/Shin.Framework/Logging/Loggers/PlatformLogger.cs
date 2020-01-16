#region Usings
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public abstract class PlatformLogger : Disposable, ILogProvider
    {
        #region Methods
        public abstract void Flush(ILogEntry entry);
        #endregion
    }
}