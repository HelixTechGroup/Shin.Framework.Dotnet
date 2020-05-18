#region Usings
using System;
using System.Threading;
#endregion

namespace Shin.Framework.Messaging
{
    public interface IMessage
    {
        SynchronizationContext Context { get; set; }
    }

    public interface IMessage<out TId> : IMessage, IId<TId> { }
}