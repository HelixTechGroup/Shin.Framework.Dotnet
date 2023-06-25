#region Usings
#endregion

#region Usings
using System;
using System.Globalization;
#endregion

namespace Shin.Framework.Logging
{
    public sealed class LogEntry : Disposable, ILogEntry, IEquatable<LogEntry>
    {
        #region Members
        private readonly LogLevel m_level;
        //private readonly object m_entryLock;
        private readonly Guid m_id;
        private readonly LogPriority m_priority;
        private string m_logDate;
        private string m_logTime;
        private string m_message;
        #endregion

        #region Properties
        public LogLevel Level
        {
            get { return m_level; }
        }

        public Guid Id
        {
            get { return m_id; }
        }

        public string LogDate
        {
            get { return m_logDate; }
        }

        public string LogTime
        {
            get { return m_logTime; }
        }

        public string Message
        {
            get
            {
                lock(m_lock)
                {
                    return m_message;
                }
            }
            set
            {
                lock(m_lock)
                {
                    m_message = value;
                    SetLogDate();
                }
            }
        }

        public LogPriority Priority
        {
            get { return m_priority; }
        }
        #endregion

        public LogEntry() : this("", LogLevel.Info, LogPriority.None) { }

        public LogEntry(LogLevel level) : this("", level, LogPriority.None) { }

        public LogEntry(LogLevel level, LogPriority priority) : this("", level, priority) { }

        public LogEntry(LogPriority priority) : this("", LogLevel.Info, priority) { }

        public LogEntry(string message, LogLevel level) : this(message, level, LogPriority.None) { }

        public LogEntry(string message, LogLevel level, LogPriority priority)
        {
            m_id = Guid.NewGuid();
            m_message = message;
            m_level = level;
            m_priority = priority;
            SetLogDate();
        }

        #region Methods
        public static bool operator ==(LogEntry left, LogEntry right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LogEntry left, LogEntry right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_id.GetHashCode();
                hashCode = (hashCode * 397) ^ m_lock.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            return !(obj is null)
                && (ReferenceEquals(this, obj)
                 || obj.GetType() == GetType()
                 && Equals((LogEntry)obj));
        }

        public bool Equals(LogEntry other)
        {
            return !(other is null)
                && (ReferenceEquals(this, other)
                 || m_id == other.Id);
        }

        private void SetLogDate()
        {
            m_logDate = DateTime.Now.ToString("yyyy-MM-dd");
            m_logTime = DateTime.Now.ToString("hh:mm:ss.fff tt");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("[{2}]:{0}:{1}",
                                 LogTime,
                                 Message,
                                 Level.ToString().ToUpper(CultureInfo.InvariantCulture));
        }
        #endregion
    }
}