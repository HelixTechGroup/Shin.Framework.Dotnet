using System.Threading;

namespace Shin.Framework.Messaging
{
    public interface IMessagePump : IInitialize, IDispose
    {
        void Initialize(CancellationToken token);
        bool Peek(out IMessage message);
        bool Poll(out IMessage message, CancellationToken ctx);
        bool Poll(out IMessage message);
        void Pump(CancellationToken ctx);
        void Pump();
        bool Wait(out IMessage message, int timeout, CancellationToken ctx);
        bool Wait(out IMessage message, int timeout);
        bool Pop(out IMessage message);
        bool Push(IMessage message);
    }

    public interface IMessagePump<T> : IInitialize, IDispose where T : IMessage
    {
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
    }
}
