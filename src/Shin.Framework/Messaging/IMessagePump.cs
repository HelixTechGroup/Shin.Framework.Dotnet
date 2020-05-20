using System.Threading;

namespace Shin.Framework.Messaging
{
    public interface IMessagePump : IInitialize, IDispose
    {

        bool Peek(out IMessage message);
        bool Poll(out IMessage message, CancellationToken ctx);
        void Pump();
        bool Wait(out IMessage message, int timeout, CancellationToken ctx);
        bool Pop(out IMessage message);
        bool Push(IMessage message);
    }

    public interface IMessagePump<T> : IMessagePump where T : IMessage
    {
        bool Peek(out T message);

        bool Poll(out T message, CancellationToken ctx);

        bool Wait(out T message, int timeout, CancellationToken ctx);

        bool Pop(out T message);

        bool Push(T message);
    }
}
