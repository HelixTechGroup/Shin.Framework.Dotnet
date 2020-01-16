#region Usings
#endregion

#region Usings
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading.Tasks;
using Shin.Framework.Collections.Concurrent;
#endregion

namespace Shin.Framework.Logging.Native
{
    public sealed class LogService : ILogService
    {
        #region Events
        public event Action<IDispose> OnDispose;

        /// <inheritdoc />
        public event EventHandler Disposing;

        /// <inheritdoc />
        event EventHandler IDispose.Disposed
        {
            add { m_disposed1 += value; }
            remove { m_disposed1 -= value; }
        }

        /// <inheritdoc />
        public event EventHandler Initializing;

        /// <inheritdoc />
        public event EventHandler Initialized;
        #endregion

        #region Members
        private const int m_queueSize = 2;
        private readonly ConcurrentList<ILogProvider> m_loggers;
        private readonly object m_logLock;
        private readonly ConcurrentQueue<ILogEntry> m_logQueue;
        private readonly Task m_logTask;
        private readonly BackgroundWorker m_logThread;
        private bool m_disposed;
        private EventHandler m_disposed1;
        private bool m_isDisposed;
        private bool m_isInitialized;
        #endregion

        #region Properties
        public bool Disposed
        {
            get { return m_disposed; }
        }

        /// <inheritdoc />
        public bool IsDisposed
        {
            get { return m_isDisposed; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return m_isInitialized; }
        }
        #endregion

        public LogService()
        {
            m_logQueue = new ConcurrentQueue<ILogEntry>();
            m_logLock = new object();
            m_loggers = new ConcurrentList<ILogProvider>();
            m_logTask = Task.CompletedTask;
            m_logThread = new BackgroundWorker();
            m_logThread.WorkerSupportsCancellation = true;
            m_logThread.DoWork += Flush;
        }

        ~LogService()
        {
            Dispose(false);
        }

        #region Methods
        /// <inheritdoc />
        public void AddLogProvider(ILogProvider logProvider)
        {
            if (!m_loggers.Contains(logProvider))
                m_loggers.Add(logProvider);
        }

        public void LogInfo(string message)
        {
            Log(new LogEntry(message, LogCategory.Info));
        }

        public void LogWarn(string message)
        {
            Log(new LogEntry(message, LogCategory.Warn));
        }

        public void LogError(string message)
        {
            Log(new LogEntry(message, LogCategory.Exception));
        }

        public void LogDebug(string message)
        {
            Log(new LogEntry(message, LogCategory.Debug));
        }

        public void LogException(Exception exception)
        {
            var trace = exception.StackTrace;

            if (exception.StackTrace.Length > 1300)
                trace = exception.StackTrace.Substring(0, 1300) + " [...] (traceback cut short)";

            LogError(string.Format("{0}\n{1}\n{2}",
                                   exception.Message,
                                   exception.Source + " raised a " + exception.GetType(),
                                   trace));
        }

        public void Log(string message, LogCategory category, LogPriority priority)
        {
            Log(new LogEntry(message, category));
        }

        public void Log(ILogEntry entry)
        {
            Enqueue(entry);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        private void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            //m_loggers.Dispose();

            OnDispose?.Invoke(this);
            m_disposed = true;
        }

        private void Enqueue(ILogEntry entry)
        {
            lock(m_logLock)
            {
                m_logQueue.Enqueue(entry);

                if (!m_logThread.IsBusy)
                    m_logThread.RunWorkerAsync();
                //Action flush = Flush;
                //if (m_logTask.IsCompleted)
                //    m_logTask = flush.OnNewThreadAsync();
            }
        }

        private void Flush(object sender, DoWorkEventArgs e)
        {
            lock(m_logLock)
            {
                if (m_logQueue.Count < m_queueSize)
                    return;

                while (m_logQueue.Count > 0)
                {
                    m_logQueue.TryDequeue(out var entry);
                    foreach (var provider in m_loggers)
                        provider.Flush(entry);

                    entry.Dispose();
                }
            }
        }
        #endregion
    }
}