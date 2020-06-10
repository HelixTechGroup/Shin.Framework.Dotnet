#region Usings
#endregion

#region Usings
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using Shin.Framework.Collections.Concurrent;
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public abstract class TextLogger : PlatformLogger
    {
        #region Members
        protected TextWriter m_writer;
        protected ConcurrentQueue<string> m_buffer;
        #endregion

        #region Methods
        public override void Flush(ILogEntry entry)
        {
            m_isBuffering = ShouldBuffer();
            var message = string.Format("[{2}]:{0}:{1}",
                                        entry.LogTime,
                                        entry.Message,
                                        entry.Category.ToString().ToUpper(CultureInfo.InvariantCulture));

            if (m_isBuffering)
                m_buffer.Enqueue(message);
            else
            {
                while (m_buffer.Count > 0)
                {
                    m_buffer.TryDequeue(out var bMessage);
                    m_writer.WriteLine(bMessage);
                }

                m_writer.WriteLine(message);
            }
        }

        protected override void DisposeManagedResources()
        {
            m_writer.Dispose();
            base.DisposeManagedResources();
        }
        #endregion
    }
}