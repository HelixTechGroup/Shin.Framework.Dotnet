#region Usings
using System;
using System.Threading;
#endregion

namespace Shin.Framework.Messaging
{
    public interface IMessagePump : IInitialize, IDispose
    {
        event EventHandler<IPumpMessage> MessagePopped;
        event EventHandler<IPumpMessage> MessagePushed;

        #region Methods
        void Initialize(CancellationToken token);

        bool Peek(out IPumpMessage message);

        bool Poll(out IPumpMessage message, CancellationToken ctx);

        bool Poll(out IPumpMessage message);

        void Pump(CancellationToken ctx);

        void Pump();

        bool Wait(out IPumpMessage message, int timeout, CancellationToken ctx);

        bool Wait(out IPumpMessage message, int timeout);

        bool Pop(out IPumpMessage message);

        bool Push(IPumpMessage message);
        #endregion
    }

    public interface IMessagePump<T> : IMessagePump where T : IPumpMessage
    {
        #region Methods
        void Initialize(CancellationToken token);

        bool Peek(out T message);

        bool Poll(out T message, CancellationToken ctx);

        bool Poll(out T message);

        void Pump(CancellationToken ctx);

        void Pump();

        bool Wait(out T message, int timeout, CancellationToken ctx);

        bool Wait(out T message, int timeout);

        bool Pop(out T message);

        bool Push(T message);
        #endregion
    }
}