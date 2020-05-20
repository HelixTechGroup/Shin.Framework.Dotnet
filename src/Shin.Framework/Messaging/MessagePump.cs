using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Shin.Framework.Messaging
{
    public abstract class MessagePump : Initializable, IMessagePump
    {
        protected ConcurrentQueue<IMessage> m_queue;
        protected ILogger m_logger;

        protected MessagePump(ILogger logger)
        {
            m_queue = new ConcurrentQueue<IMessage>();
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
        public abstract void Pump();

        /// <inheritdoc />
        public virtual bool Wait(out IMessage message, int timeout, CancellationToken ctx)
        {
            var startTime = DateTimeOffset.Now;
            while (true)
            {
                if (ctx.IsCancellationRequested)
                {
                    message = null;
                    return false;
                }

                Pump();
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
    }
}
