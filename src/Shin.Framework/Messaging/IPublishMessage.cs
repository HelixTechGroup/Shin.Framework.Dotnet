namespace Shin.Framework.Messaging 
{
    public interface IPublishMessage
    {
        void Publish<TMessage>(params object[] arguments) where TMessage : IMessage, new();
    }

    public interface IPublishMessage<in TPayload> : IPublishMessage
    {
        void Publish<TMessage>(TPayload payload) where TMessage : IMessage, new();
    }
}