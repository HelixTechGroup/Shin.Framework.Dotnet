#region Usings
#endregion

using System;
using System.Runtime.Serialization;

namespace Shin.Framework.IoC.Native.DependencyInjection.Exceptions
{
    [Serializable]
    public class IoCRegistrationException : Exception
    {
        public IoCRegistrationException() { }

        public IoCRegistrationException(string message) : base(message) { }

        public IoCRegistrationException(string message, Exception innerException)
            : base(message, innerException) { }

        protected IoCRegistrationException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}