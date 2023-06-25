#region Usings
#endregion

#region Usings
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using Shin.Framework.Messaging.Messages;
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public abstract class TextLogger : PlatformLogger
    {
        #region Members
        protected ConcurrentQueue<string> m_buffer;
        protected TextWriter m_writer;
        #endregion

        protected TextLogger()
        {
            m_buffer = new ConcurrentQueue<string>();
        }

        #region Methods
        protected override void PlatformFlush(ILogEntry entry)
        {
            //m_isBuffering = ShouldBuffer();
            m_isBuffering = ShouldBuffer();
            var message = entry.Message;
            
            switch (entry.Level)
            {
                case LogLevel.None:
                    break;
                default:
                    message = $"{entry}\r\n";
                    break;
            }

            if (!m_isBuffering)
            {
                while (m_buffer.Count > 0)
                {
                    m_buffer.TryDequeue(out var bMessage);
                    m_writer.WriteLine(bMessage);
                }

                m_writer.Write(message);
            }
            else
                m_buffer.Enqueue(message);
        }

        protected override void DisposeManagedResources()
        {
            m_writer.Dispose();
            base.DisposeManagedResources();
        }
        #endregion
    }
}