#region Usings
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Shin.Framework.Collections.Concurrent;
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public class ConsoleLogger : TextLogger
    {
        public ConsoleLogger()
        {
            if (IsConsoleAvailable())
                m_writer = Console.Out;
            else
                m_isBuffering = true;
        }

        protected override bool ShouldBuffer()
        {
            if (!IsConsoleAvailable())
                return false;

            m_writer = Console.Out;
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