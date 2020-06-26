#region Usings
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
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
                return true;

            m_writer = Console.Out;
            return false;
        }

        protected bool IsConsoleAvailable()
        {
            try
            {
                if (!Environment.UserInteractive)
                    return Console.In is StreamReader;

                if (Console.OpenStandardInput(1) == Stream.Null)
                    return false;

                var key = Console.WindowHeight;
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