using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Messaging
{
    public interface IPublish
    {
        void Publish(params object[] arguments);
    }

    public interface IPublish<in TPayload> : IPublish
    {
        void Publish(TPayload payload);
    }
}
