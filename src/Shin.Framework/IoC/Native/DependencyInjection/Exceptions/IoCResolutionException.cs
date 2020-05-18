using System;
using System.Runtime.Serialization;

namespace Shield.Framework.IoC.Native.DependencyInjection.Exceptions
{
    [Serializable]
    public class IoCResolutionException : Exception
    {
        public IoCResolutionException() { }

        public IoCResolutionException(string message) : base(message) { }

        public IoCResolutionException(string message, Exception innerException)
            : base(message, innerException) { }

        protected IoCResolutionException(SerializationInfo info, StreamingContext context) 
            : base(info, context) { }
    }
}
