#region Usings
using System;
using System.Runtime.Serialization;
#endregion

namespace Shin.Framework.Exceptions
{
    [Serializable]
    public class ExceptionNotRegisteredException : Exception
    {
        #region Properties
        public Type ExceptionType { get; set; }
        #endregion

        public ExceptionNotRegisteredException()
            : this(null) { }

        public ExceptionNotRegisteredException(string message)
            : this(null, message) { }

        public ExceptionNotRegisteredException(string message, Exception innerException)
            : this(null, message, innerException) { }

        public ExceptionNotRegisteredException(Type exceptionType, string message)
            : this(exceptionType, message, null) { }

        public ExceptionNotRegisteredException(Type exceptionType, string message, Exception innerException)
            : base(message, innerException)
        {
            ExceptionType = exceptionType;
        }

        protected ExceptionNotRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}