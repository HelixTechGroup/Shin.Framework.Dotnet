#region Usings
using System;
#endregion

namespace Shin.Framework.Messaging
{
    public interface IActionMessage : IMessage, IPublish, ISubscribe<Action> { }

    public interface IActionMessage<TPayload> : IMessage, IPublish<TPayload>, ISubscribe<Action<TPayload>> { }
}