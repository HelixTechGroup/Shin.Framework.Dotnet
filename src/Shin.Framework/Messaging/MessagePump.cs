using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Shin.Framework.Messaging
{
    public abstract class MessagePump : Initializable, IMessagePump
    {
        protected ConcurrentQueue<IMessage> m_queue;
        protected CancellationTokenSource m_tokenSource;
        protected CancellationToken m_token;
        protected ILogger m_logger;

        protected MessagePump(ILogger logger)
        {
            m_queue = new ConcurrentQueue<IMessage>();
            m_token = new CancellationToken();
        }

        protected override void InitializeResources()
        {
            base.InitializeResources();
            m_tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_token);
        }

        public void Initialize(CancellationToken token)
        {
            m_token = token;
            Initialize();
        }

        /// <inheritdoc />
        public virtual bool Peek(out IMessage message)
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
        public virtual bool Poll(out IMessage message, CancellationToken ctx)
        {
            return Wait(out message, 0, ctx);
        }

        /// <inheritdoc />
        public bool Poll(out IMessage message)
        {
            return Poll(out message, CancellationToken.None);
        }

        /// <inheritdoc />
        public abstract void Pump(CancellationToken ctx);

        /// <inheritdoc />
        public abstract void Pump();

        /// <inheritdoc />
        public virtual bool Wait(out IMessage message, int timeout, CancellationToken ctx)
        {
            AddCancellationToken(ctx);

            var startTime = DateTimeOffset.Now;
            while (true)
            {
                if (m_tokenSource.Token.IsCancellationRequested)
                {
                    message = null;
                    return false;
                }

                Pump(ctx);
                if (timeout == 0)
                    return Pop(out message);

                switch (Pop(out message))
                {
                    case false:
                        if (timeout > 0 && (DateTimeOffset.Now.Subtract(startTime).TotalMilliseconds > timeout))
                            return false;

                        Thread.Sleep(10);
                        break;
                    default:
                        return true;
                }
            }
        }

        /// <inheritdoc />
        public bool Wait(out IMessage message, int timeout)
        {
            return Wait(out message, timeout, CancellationToken.None);
        }

        /// <inheritdoc />
        public virtual bool Push(IMessage message)
        {
            try
            {
                m_queue.Enqueue(message);
            }
            catch (Exception ex)
            {
                m_logger.LogException(ex);
                return false;
            }

            return true;
        }

        public virtual bool Pop(out IMessage message)
        {
            try
            {
                return m_queue.TryDequeue(out message);
            }
            catch (Exception ex)
            {
                m_logger.LogException(ex);
                message = null;
                return false;
            }
        }

        protected override void DisposeManagedResources()
        {
            if (!m_tokenSource.IsCancellationRequested)
                m_tokenSource.Cancel();

            base.DisposeManagedResources();
            m_tokenSource.Dispose();
        }

        protected void AddCancellationToken(CancellationToken ctx)
        {
            if (ctx != CancellationToken.None)
                m_tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_token, ctx);
        }
    }

    public abstract class MessagePump<T> : Initializable, IMessagePump<T> where T : IMessage
    {
        protected ConcurrentQueue<T> m_queue;
        protected CancellationTokenSource m_tokenSource;
        protected CancellationToken m_token;
        protected ILogger m_logger;

        protected MessagePump(ILogger logger)
        {
            m_queue = new ConcurrentQueue<T>();
            m_token = new CancellationToken();
        }

        protected override void InitializeResources()
        {
            base.InitializeResources();
            m_tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_token);
        }

        public void Initialize(CancellationToken token)
        {
            m_token = token;
            Initialize();
        }

        /// <inheritdoc />
        public virtual bool Peek(out T message)
        {
            try
            {
                return m_queue.TryPeek(out message);
            }
            catch (Exception ex)
            {
                m_logger.LogException(ex);
                message = default(T);
                return false;
            }
        }

        /// <inheritdoc />
        public virtual bool Poll(out T message, CancellationToken ctx)
        {
            return Wait(out message, 0, ctx);
        }

        /// <inheritdoc />
        public bool Poll(out T message)
        {
            return Poll(out message, CancellationToken.None);
        }

        /// <inheritdoc />
        public abstract void Pump(CancellationToken ctx);

        /// <inheritdoc />
        public abstract void Pump();

        /// <inheritdoc />
        public virtual bool Wait(out T message, int timeout, CancellationToken ctx)
        {
            AddCancellationToken(ctx);

            var startTime = DateTimeOffset.Now;
            while (true)
            {
                if (m_tokenSource.Token.IsCancellationRequested)
                {
                    message = default(T);
                    return false;
                }

                Pump(ctx);
                if (timeout == 0)
                    return Pop(out message);

                switch (Pop(out message))
                {
                    case false:
                        if (timeout > 0 && (DateTimeOffset.Now.Subtract(startTime).TotalMilliseconds > timeout))
                            return false;

                        Thread.Sleep(10);
                        break;
                    default:
                        return true;
                }
            }
        }

        /// <inheritdoc />
        public bool Wait(out T message, int timeout)
        {
            return Wait(out message, timeout, CancellationToken.None);
        }

        /// <inheritdoc />
        public virtual bool Push(T message)
        {
            try
            {
                m_queue.Enqueue(message);
            }
            catch (Exception ex)
            {
                m_logger.LogException(ex);
                return false;
            }

            return true;
        }

        public virtual bool Pop(out T message)
        {
            try
            {
                return m_queue.TryDequeue(out message);
            }
            catch (Exception ex)
            {
                m_logger.LogException(ex);
                message = default(T);
                return false;
            }
        }

        protected override void DisposeManagedResources()
        {
            if (!m_tokenSource.IsCancellationRequested)
                m_tokenSource.Cancel();

            base.DisposeManagedResources();
            m_tokenSource.Dispose();
        }

        protected void AddCancellationToken(CancellationToken ctx)
        {
            if (ctx != CancellationToken.None)
                m_tokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_token, ctx);
        }
    }
}
