#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shin.Framework.Exceptions;
#endregion

namespace Shin.Framework
{
    public static partial class Throw
    {
#region Methods
        public static void IfNullArgument<T>(T obj,
                                             string message = null,
                                             object[] args = null,
                                             StackFrame frame = default,
                                             params KeyValuePair<string, object>[] data)
        {
            if (obj == null) throw ExceptionProvider.GenerateException<ArgumentNullException>(message, args, frame,data);
        }

        public static void IfNull<T, TException>(T obj,
                                                 string message = null,
                                                 object[] args = null,
                                                 StackFrame frame = default,
                                                 params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            if (obj == null) throw ExceptionProvider.GenerateException<TException>(message, args, frame,data);
        }

        public static ThrowExceptionSelector IfNull<T>(T obj) { return new ThrowExceptionSelector(obj == null); }

        public static void IfNullOrEmpty<TException>(string obj,
                                                     string message = null,
                                                     object[] args = null,
                                                     StackFrame frame = default,
                                                     params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            if (string.IsNullOrWhiteSpace(obj)) throw ExceptionProvider.GenerateException<TException>(message, args, frame,data);
        }

        public static ThrowExceptionSelector IfNullOrEmpty(string obj) { return new ThrowExceptionSelector(string.IsNullOrWhiteSpace(obj)); }
#endregion
    }
}