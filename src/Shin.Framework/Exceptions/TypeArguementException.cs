#region Usings
using System;
using System.Runtime.Serialization;
#endregion

namespace Shin.Framework.Exceptions
{
    [Serializable]
    public class TypeArgumentException : Exception
    {
        public TypeArgumentException() { }

        public TypeArgumentException(string message)
            : base(message) { }

        public TypeArgumentException(string message, Exception inner)
            : base(message, inner) { }

        protected TypeArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}