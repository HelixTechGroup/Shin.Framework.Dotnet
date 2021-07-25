#region Usings
#endregion

using System;

namespace Shin.Framework.Messaging
{
    public interface IMessage : IEquatable<IMessage>
    {
    }

    public interface IMessage<TId> : IMessage, IId<TId>, IEquatable<IMessage<TId>> { }
}