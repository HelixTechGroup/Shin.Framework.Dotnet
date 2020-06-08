#region Usings
#endregion

#region Usings
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Shin.Framework.Collections.Concurrent;
#endregion

namespace Shin.Framework.Logging.Native
{
    public sealed class Logger : Initializable, ILogger
    {
        #region Members
        private const int m_queueSize = 2;
        private readonly ConcurrentList<ILogProvider> m_loggers;
        private readonly object m_logLock;
        private readonly ConcurrentQueue<ILogEntry> m_logQueue;
        private readonly Task m_logTask;
        private readonly BackgroundWorker m_logThread;
        #endregion

        public Logger()
        {
            m_logQueue = new ConcurrentQueue<ILogEntry>();
            m_logLock = new object();
            m_loggers = new ConcurrentList<ILogProvider>();
            m_logTask = Task.CompletedTask;
            m_logThread = new BackgroundWorker();
            m_logThread.WorkerSupportsCancellation = true;
            m_logThread.DoWork += Flush;
        }

        #region Methods
        /// <inheritdoc />
        protected override void InitializeResources()
        {
            base.InitializeResources();
            m_logThread.RunWorkerAsync();
        }

        /// <inheritdoc />
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            m_logThread.CancelAsync();
            m_logThread.Dispose();
        }

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

        private void Enqueue(ILogEntry entry)
        {
            lock(m_logLock)
            {
                m_logQueue.Enqueue(entry);

                //if (!m_logThread.IsBusy)
                //    m_logThread.RunWorkerAsync();
                //Action flush = Flush;
                //if (m_logTask.IsCompleted)
                //    m_logTask = flush.OnNewThreadAsync();
            }
        }

        private void Flush(object sender, DoWorkEventArgs e)
        {
            //lock(m_logLock)
            //{
            while(true)
            { 
                //if (m_logQueue.Count < m_queueSize)
                while (m_logQueue.Count == 0)
                    Thread.Sleep(20);

                while (m_logQueue.Count > 0)
                {
                    m_logQueue.TryDequeue(out var entry);
                    lock (m_logLock)
                    {
                        foreach (var provider in m_loggers)
                            provider.Flush(entry);

                        entry.Dispose();
                    }
                }
            }
            //}
        }
        #endregion
    }
}