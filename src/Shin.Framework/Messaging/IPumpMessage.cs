using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Messaging
{
    public interface IPumpMessage : IMessage
    {
    }

    public interface IPumpMessage<TId> : IPumpMessage, IMessage<TId>
    {

    }
}
