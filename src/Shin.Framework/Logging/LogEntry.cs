#region Usings
#endregion

#region Usings
using System;
#endregion

namespace Shin.Framework.Logging
{
    public sealed class LogEntry : ILogEntry, IEquatable<LogEntry>
    {
        #region Events
        public event Action<IDispose> OnDispose;

        /// <inheritdoc />
        public event EventHandler Disposing;

        /// <inheritdoc />
        public event EventHandler Disposed;
        #endregion

        #region Members
        private readonly LogCategory m_category;
        private readonly object m_entryLock;
        private readonly Guid m_id;
        private readonly LogPriority m_priority;
        private bool m_isDisposed;
        private string m_logDate;
        private string m_logTime;
        private string m_message;
        #endregion

        #region Properties
        public LogCategory Category
        {
            get { return m_category; }
        }

        public Guid Id
        {
            get { return m_id; }
        }

        public bool IsDisposed
        {
            get { return m_isDisposed; }
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
                lock(m_entryLock)
                {
                    return m_message;
                }
            }
            set
            {
                lock(m_entryLock)
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

        public LogEntry() : this("", LogCategory.Info, LogPriority.None) { }

        public LogEntry(LogCategory level) : this("", level, LogPriority.None) { }

        public LogEntry(LogCategory level, LogPriority priority) : this("", level, priority) { }

        public LogEntry(LogPriority priority) : this("", LogCategory.Info, priority) { }

        public LogEntry(string message, LogCategory level) : this(message, level, LogPriority.None) { }

        public LogEntry(string message, LogCategory level, LogPriority priority)
        {
            m_id = Guid.NewGuid();
            m_entryLock = new object();
            m_message = message;
            m_category = level;
            m_priority = priority;
            SetLogDate();
        }

        ~LogEntry()
        {
            Dispose(false);
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
                hashCode = (hashCode * 397) ^ m_entryLock.GetHashCode();
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Equals(LogEntry other)
        {
            return !(other is null)
                && (ReferenceEquals(this, other)
                 || m_id == other.Id);
        }

        private void Dispose(bool disposing)
        {
            if (m_isDisposed)
                return;

            OnDispose?.Invoke(this);
            m_isDisposed = true;
        }

        private void SetLogDate()
        {
            m_logDate = DateTime.Now.ToString("yyyy-MM-dd");
            m_logTime = DateTime.Now.ToString("hh:mm:ss.fff tt");
        }
        #endregion
    }
}