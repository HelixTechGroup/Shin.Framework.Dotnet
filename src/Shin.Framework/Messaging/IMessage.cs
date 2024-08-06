#region Usings
#endregion

using System;

namespace Shin.Messaging
{
    public interface IMessage : IEquatable<IMessage>
    {
    }

    public interface IMessage<TId> : IMessage, IId<TId>, IEquatable<IMessage<TId>> { }
}