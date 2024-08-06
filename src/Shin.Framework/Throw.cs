#region Usings
#endregion

#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Shin.Exceptions;
#endregion

namespace Shin
{
    public static partial class Throw
    {
        #region Methods
        public static ThrowExceptionBuilder Exception()
        {
            return new ThrowExceptionBuilder(true);
        }

        public static void Exception<TException>(string message = null,
                                                 object[] args = null,
                                                 StackFrame frame = default,
                                                 params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            throw ExceptionProvider.GenerateException<TException>(message, args, frame,data);
        }

        public static void If<TException>(Func<bool> predicate,
                                          string message = null,
                                          object[] args = null,
                                          StackFrame frame = default,
                                          params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            if (predicate())
                throw ExceptionProvider.GenerateException<TException>(message, args, frame,data);
        }

        public static void If<TException>(bool predicate,
                                          string message = null,
                                          object[] args = null,
                                          StackFrame frame = default,
                                          params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            if (predicate)
                throw ExceptionProvider.GenerateException<TException>(message, args, frame,data);
        }

        public static ThrowExceptionBuilder If(Func<bool> predicate)
        {
            return new ThrowExceptionBuilder(predicate());
        }

        public static ThrowExceptionBuilder If(bool predicate)
        {
            return new ThrowExceptionBuilder(predicate);
        }

        public static void IfNot<TException>(Func<bool> predicate,
                                             string message = null,
                                             object[] args = null,
                                             StackFrame frame = default,
                                             params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            if (!predicate())
                throw ExceptionProvider.GenerateException<TException>(message, args, frame,data);
        }

        public static void IfNot<TException>(bool predicate,
                                             string message = null,
                                             object[] args = null,
                                             StackFrame frame = default,
                                             params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            if (!predicate)
                throw ExceptionProvider.GenerateException<TException>(message, args, frame,data);
        }

        public static ThrowExceptionBuilder IfNot(Func<bool> predicate)
        {
            return new ThrowExceptionBuilder(!predicate());
        }

        public static ThrowExceptionBuilder IfNot(bool predicate)
        {
            return new ThrowExceptionBuilder(!predicate);
        }
        #endregion
    }
}