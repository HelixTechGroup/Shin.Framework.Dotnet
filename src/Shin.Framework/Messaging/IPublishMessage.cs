namespace Shin.Framework.Messaging
{
    public interface IPublishMessage
    {
        #region Methods
        void Publish<TMessage>(params object[] arguments) where TMessage : IMessage, new();
        #endregion
    }

    public interface IPublishMessage<in TPayload> : IPublishMessage
    {
        #region Methods
        void Publish<TMessage>(TPayload payload) where TMessage : IMessage, new();
        #endregion
    }
}