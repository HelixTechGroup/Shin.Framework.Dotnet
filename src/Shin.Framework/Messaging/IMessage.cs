#region Usings
using System;
using System.Threading;
#endregion

namespace Shin.Framework.Messaging
{
    public interface IMessage
    {
    }

    public interface IMessage<out TId> : IMessage, IId<TId> { }
}