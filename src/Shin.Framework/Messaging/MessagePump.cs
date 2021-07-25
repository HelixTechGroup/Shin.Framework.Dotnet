#region Usings
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.Extensions;
#endregion

namespace Shin.Framework.Messaging
{
    public abstract class MessagePump : Initializable, IMessagePump
    {
        #region Events
        public event EventHandler<IPumpMessage> MessagePopped;
        public event EventHandler<IPumpMessage> MessagePushed;
        #endregion

        #region Members
        protected readonly ConcurrentList<int> m_tokens;
        protected ILogger m_logger;
        protected IPumpMessage m_last;
        protected ConcurrentQueue<IPumpMessage> m_queue;
        protected ConcurrentQueue<IPumpMessage> m_deferred;
        protected CancellationToken m_token;
        protected CancellationTokenSource m_tokenSource;
        #endregion

        protected MessagePump(ILogger logger)
        {
            m_queue = new ConcurrentQueue<IPumpMessage>();
            m_deferred = new ConcurrentQueue<IPumpMessage>();
            m_token = new CancellationToken();
            m_tokens = new ConcurrentList<int>();
            m_logger = logger;
        }

        #region Methods
        public void Initialize(CancellationToken token)
        {
            m_token = token;
            Initialize();
        }

        /// <inheritdoc />
        public virtual bool Peek(out IPumpMessage message)
        {
            try
            {
                return m_queue.TryPeek(out message);
            }
            catch (Exception ex)
            {
                m_logger.LogException(ex);
                message = null;
                return false;
            }
        }

        /// <inheritdoc />
        public virtual bool Poll(out IPumpMessage message, CancellationToken ctx)
        {
            return Wait(out message, 0, ctx);
        }

        /// <inheritdoc />
        public bool Poll(out IPumpMessage message)
        {
            return Poll(out message, CancellationToken.None);
        }

        /// <inheritdoc />
        public abstract void Pump(CancellationToken ctx);

        /// <inheritdoc />
        public abstract void Pump();

        /// <inheritdoc />
        public virtual bool Wait(out IPumpMessage message, int timeout, CancellationToken ctx)
        {
            AddCancellationToken(ctx);

            var startTime = DateTimeOffset.Now;
            do
            {
                Pump(ctx);
                if (timeout == 0)
                    return Pop(out message);

                switch (Pop(out message))
                {
                    case false:
                        if (timeout > 0 && DateTimeOffset.Now.Subtract(startTime).TotalMilliseconds > timeout)
                            return false;

                        Thread.Sleep(10);
                        break;
                    default:
                        return true;
                }
            } while (!m_tokenSource.IsCancellationRequested);

            message = default;
            return false;
        }

        /// <inheritdoc />
        public bool Wait(out IPumpMessage message, int timeout)
        {
            return Wait(out message, timeout, CancellationToken.None);
        }

        /// <inheritdoc />
        public virtual bool Push(IPumpMessage message)
        {
            try
            {
                if (m_deferred.ToArray().ToList().Contains(message))
                    return true;
                m_queue.Enqueue(message);
                m_deferred.Enqueue(message);
                MessagePushed.Raise(this, message);
                m_last = message;
            }
            catch (Exception ex)
            {
                m_logger.LogException(ex);
                return false;
            }

            return true;
        }

        public virtual bool Pop(out IPumpMessage message)
        {
            try
            {
                if (m_queue.TryDequeue(out message))
                {
                    m_deferred.TryDequeue(out var res);

                    MessagePopped.Raise(this, message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                m_logger.LogException(ex);
                message = null;
                return false;
            }

            return false;
        }

        protected override void DisposeManagedResources()
        {
            if (!m_tokenSource.IsCancellationRequested)
                m_tokenSource.Cancel();

            base.DisposeManagedResources();
            m_tokenSource.Dispose();
            MessagePushed.Dispose();
            MessagePopped.Dispose();
        }

        protected override void InitializeResources()
        {
            base.InitializeResources();
            m_tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_token);
        }

        protected void AddCancellationToken(CancellationToken ctx)
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

    //public abstract class MessagePump<T> : MessagePump, IMessagePump<T> where T : IMessage
    //{
    //    #region Events
    //    public event EventHandler<T> MessagePopped;
    //    public event EventHandler<T> MessagePushed;
    //    #endregion

    //    #region Members
    //    protected readonly ConcurrentList<int> m_tokens;
    //    protected ILogger m_logger;

    //    protected ConcurrentQueue<T> m_queue;
    //    protected CancellationToken m_token;
    //    protected CancellationTokenSource m_tokenSource;
    //    #endregion

    //    protected MessagePump(ILogger logger) : base(logger)
    //    {
    //        m_queue = new ConcurrentQueue<T>();
    //        m_token = new CancellationToken();
    //        m_tokenSource = new CancellationTokenSource();
    //        m_tokens = new ConcurrentList<int>();
    //    }

    //    #region Methods
    //    /// <inheritdoc />
    //    public virtual bool Peek(out T message)
    //    {
    //        try
    //        {
    //            return m_queue.TryPeek(out message);
    //        }
    //        catch (Exception ex)
    //        {
    //            m_logger.LogException(ex);
    //            message = default;
    //            return false;
    //        }
    //    }

    //    /// <inheritdoc />
    //    public virtual bool Poll(out T message, CancellationToken ctx)
    //    {
    //        return Wait(out message, 0, ctx);
    //    }

    //    /// <inheritdoc />
    //    public bool Poll(out T message)
    //    {
    //        return Poll(out message, CancellationToken.None);
    //    }

    //    /// <inheritdoc />
    //    public virtual bool Wait(out T message, int timeout, CancellationToken ctx)
    //    {
    //        AddCancellationToken(ctx);

    //        var startTime = DateTimeOffset.Now;
    //        do
    //        {
    //            Pump(ctx);
    //            if (timeout == 0)
    //                return Pop(out message);

    //            switch (Pop(out message))
    //            {
    //                case false:
    //                    if (timeout > 0 && DateTimeOffset.Now.Subtract(startTime).TotalMilliseconds > timeout)
    //                        return false;

    //                    Thread.Sleep(10);
    //                    break;
    //                default:
    //                    return true;
    //            }
    //        } while (!m_tokenSource.IsCancellationRequested);

    //        message = default;
    //        return false;
    //    }

    //    /// <inheritdoc />
    //    public bool Wait(out T message, int timeout = -1)
    //    {
    //        return Wait(out message, timeout, CancellationToken.None);
    //    }

    //    /// <inheritdoc />
    //    public virtual bool Push(T message)
    //    {
    //        try
    //        {
    //            m_queue.Enqueue(message);
    //            MessagePushed.Raise(this, message);
    //        }
    //        catch (Exception ex)
    //        {
    //            m_logger.LogException(ex);
    //            return false;
    //        }

    //        return true;
    //    }

    //    public virtual bool Pop(out T message)
    //    {
    //        try
    //        {
    //            if (m_queue.TryDequeue(out message))
    //            {
    //                MessagePopped.Raise(this, message);
    //                return true;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            m_logger.LogException(ex);
    //            message = default;
    //            return false;
    //        }

    //        return false;
    //    }

    //    protected override void DisposeManagedResources()
    //    {
    //        MessagePushed.Dispose();
    //        MessagePopped.Dispose();
    //    }
    //    #endregion
    //}
}