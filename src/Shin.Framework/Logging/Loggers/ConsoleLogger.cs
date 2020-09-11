#region Usings
using System;
using System.Diagnostics;
using System.IO;
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public class ConsoleLogger : TextLogger
    {
        #region Members
        private readonly bool m_checkConsole;
        #endregion

        public ConsoleLogger(bool checkConsole = true)
        {
            m_checkConsole = checkConsole;
            if (IsConsoleAvailable())
                m_writer = Console.Out;
            else
                m_isBuffering = true;
        }

        #region Methods
        protected override bool ShouldBuffer()
        {
            if (!IsConsoleAvailable())
                return true;

            m_writer = Console.Out;

            return false;
        }

        protected bool IsConsoleAvailable()
        {
            if (!m_checkConsole)
                return true;

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
        #endregion
    }
}