#region Usings
#endregion

#region Usings
using Shin.Framework.Messaging;
#endregion

namespace Shin.Framework
{
    public interface IMessageAggregator : IInitialize, IDispose
    {
        #region Methods
        bool MessageExists<T>() where T : IMessage, new();

        T GetMessage<T>() where T : IMessage, new();

        void RemoveMessage<T>() where T : IMessage, new();
        #endregion
    }
}