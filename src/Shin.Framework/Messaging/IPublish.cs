namespace Shin.Framework.Messaging
{
    public interface IPublish
    {
        #region Methods
        void Publish(params object[] arguments);
        #endregion
    }

    public interface IPublish<in TPayload> : IPublish
    {
        #region Methods
        void Publish(TPayload payload);
        #endregion
    }
}