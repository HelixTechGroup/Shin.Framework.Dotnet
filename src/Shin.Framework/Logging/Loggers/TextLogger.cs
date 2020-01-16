#region Usings
#endregion

#region Usings
using System.Globalization;
using System.IO;
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public abstract class TextLogger : PlatformLogger
    {
        #region Members
        protected TextWriter m_writer;
        #endregion

        #region Methods
        public override void Flush(ILogEntry entry)
        {
            var message = string.Format("[{2}]:{0}:{1}",
                                        entry.LogTime,
                                        entry.Message,
                                        entry.Category.ToString().ToUpper(CultureInfo.InvariantCulture));

            m_writer.WriteLine(message);
        }

        protected override void DisposeManagedResources()
        {
            m_writer.Dispose();
            base.DisposeManagedResources();
        }
        #endregion
    }
}