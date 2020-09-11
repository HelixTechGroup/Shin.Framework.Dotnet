#region Usings
#endregion

#region Usings
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Shin.Framework.Collections.Concurrent;
#endregion

namespace Shin.Framework.Logging.Native
{
    public sealed class Logger : Initializable, ILogger
    {
        #region Members
        private const int m_maxQueueSize = 1000;
        private const int m_maxStackTraceSize = 1500;
        private int m_queueSize = 10;
        private int m_currentQueueSize = 0;
        private readonly ConcurrentList<ILogProvider> m_loggers;
        private readonly object m_logLock;
        private readonly ConcurrentQueue<ILogEntry> m_logQueue;
        private readonly Task m_logTask;
        private readonly BackgroundWorker m_logWorker;
        private readonly Thread m_thread;
        private readonly ConcurrentList<int> m_tokens;
        private CancellationToken m_token;
        private CancellationTokenSource m_tokenSource;
        #endregion

        #region Properties
        public Thread Thread
        {
            get { return m_thread; }
        }

        public int QueueSize
        {
            get { return m_queueSize; }
            set { AdjustQueue(value); }
        }

        private void AdjustQueue(in int requestedValue)
        {
            if (requestedValue > m_maxQueueSize)
                Throw.Exception().InvalidOperationException();

            m_queueSize = requestedValue;
        }
        #endregion

        public Logger()
        {
            m_logQueue = new ConcurrentQueue<ILogEntry>();
            m_logLock = new object();
            m_loggers = new ConcurrentList<ILogProvider>();
            //m_logTask = Task.CompletedTask;
            //m_logWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            //m_logWorker.DoWork += Flush;
            //m_logTask = new Task(Flush);
            m_thread = new Thread(Flush);
            m_token = new CancellationToken();
            m_tokenSource = new CancellationTokenSource();
            m_tokens = new ConcurrentList<int>();
        }

        public Logger(CancellationToken token)
        {

        }

        #region Methods
        public void Initialize(CancellationToken token)
        {
            if (m_isInitialized)
                return;

            m_token = token;
            Initialize();
        }

        /// <inheritdoc />
        public void AddLogProvider(ILogProvider logProvider)
        {
            if (!m_loggers.Contains(logProvider))
                m_loggers.Add(logProvider);
        }

        public void LogNone(string message)
        {
            Log(new LogEntry(message, LogLevel.None));
        }

        public void LogInfo(string message)
        {
            Log(new LogEntry(message, LogLevel.Info));
        }

        public void LogWarn(string message)
        {
            Log(new LogEntry(message, LogLevel.Warn));
        }

        public void LogError(string message)
        {
            Log(new LogEntry(message, LogLevel.Exception));
        }

        public void LogDebug(string message)
        {
            Log(new LogEntry(message, LogLevel.Debug));
        }

        public void LogException(Exception exception)
        {
            var trace = exception.StackTrace;

            if (exception.StackTrace.Length > m_maxStackTraceSize)
                trace = exception.StackTrace.Substring(0, m_maxStackTraceSize) + " [...] (stacktrace cut short)";

            LogError(string.Format("{0}\n{1}\n{2}",
                                   exception.Message,
                                   exception.Source + " raised a " + exception.GetType(),
                                   trace));
        }

        public void Log(string message, LogLevel category, LogPriority priority)
        {
            Log(new LogEntry(message, category));
        }

        public void Log(ILogEntry entry)
        {
            Enqueue(entry);
        }

        /// <inheritdoc />
        protected override void DisposeManagedResources()
        {
            if (!m_tokenSource.IsCancellationRequested)
                m_tokenSource.Cancel();

            m_thread.Join();

            base.DisposeManagedResources();

            //m_logWorker.CancelAsync();
            //m_logWorker.Dispose();
            m_tokenSource.Dispose();

            foreach (var logger in m_loggers)
                logger.Dispose();
        }

        /// <inheritdoc />
        protected override void InitializeResources()
        {
            base.InitializeResources();
            AddCancellationToken(m_token);
            //m_logTask.Start();
            m_thread.Start(m_tokenSource.Token);
            //m_logWorker.RunWorkerAsync();
        }

        private void Enqueue(ILogEntry entry)
        {
            //lock(m_logLock)
            //{
            m_logQueue.Enqueue(entry);

            //if (!m_logThread.IsBusy)
            //    m_logThread.RunWorkerAsync();
            //Action flush = Flush;
            //if (m_logTask.IsCompleted)
            //    m_logTask = flush.OnNewThreadAsync();
            //}
        }

        //private void Flush(object sender, DoWorkEventArgs e)
        private void Flush(object sender)
        {
            var token = (CancellationToken)sender;
            //lock(m_logLock)
            //{
            do
            {
                //if (m_logQueue.Count < m_queueSize)
                while (m_logQueue.Count < m_queueSize && !token.IsCancellationRequested) /*&& !m_logWorker.CancellationPending)*/
                    Thread.Sleep(250);

                while (m_logQueue.Count > 0)
                {
                    m_logQueue.TryDequeue(out var entry);
                    lock(m_logLock)
                    {
                        foreach (var provider in m_loggers)
                            provider.Flush(entry);
                    }

                    entry.Dispose();
                }
            } while (!token.IsCancellationRequested); //(!m_logWorker.CancellationPending);

            //}
        }

        private void AddCancellationToken(CancellationToken ctx)
        {
            if (ctx == CancellationToken.None || m_token.IsCancellationRequested)
                return;

            if (m_tokens.Contains(ctx.GetHashCode()))
                return;

            m_tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_token, ctx);
            m_tokens.Add(ctx.GetHashCode());
        }
        #endregion
    }
}