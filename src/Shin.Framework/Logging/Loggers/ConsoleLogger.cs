#region Usings
using System;
using System.Diagnostics;
using Shin.Framework.Collections.Concurrent;
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public class ConsoleLogger : TextLogger
    {
        public ConsoleLogger()
        {
            m_buffer = new ConcurrentList<string>();

            if (IsConsoleAvailable())
                m_writer = Console.Out;
            else
                m_isBuffering = true;
        }

        protected override bool ShouldBuffer()
        {
            if (IsConsoleAvailable())
                return true;

            return false;
        }

        protected bool IsConsoleAvailable()
        {
            try
            {
                var key = Console.KeyAvailable;
                return true;
            }
            catch
            {
                Trace.WriteLine("Console is not available.");
                return false;
            }
        }
    }
}