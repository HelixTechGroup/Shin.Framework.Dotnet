#region Usings
using System;
#endregion

namespace Shin.Framework.Logging.Loggers
{
    public class ConsoleLogger : TextLogger
    {
        public ConsoleLogger()
        {
            m_writer = Console.Out;
        }
    }
}