namespace Shin.Framework.Messaging
{
    public interface IMessagePump : IInitialize, IDispose
    {

        int Peek(out IMessage message);
        int Poll(out IMessage message);
        void Pump();
        int Wait(out IMessage message, int timeout);
        int Push(IMessage message);
    }
}
