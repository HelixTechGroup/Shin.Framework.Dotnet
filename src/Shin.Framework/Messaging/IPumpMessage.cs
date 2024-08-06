namespace Shin.Messaging
{
    public interface IPumpMessage : IMessage
    {
    }

    public interface IPumpMessage<TId> : IPumpMessage, IMessage<TId>
    {

    }
}
