#region Usings
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.Extensions;
#endregion

namespace Shin.Framework.Exceptions
{
    public static partial class ExceptionProvider
    {
        #region Methods
        public static TException GenerateArgumentException<TException>(string argumentName,
                                                                       string message = null,
                                                                       params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            return GenerateException(ArgumentExceptionFactory<TException>(argumentName, message), data);
        }

        public static TException GenerateException<TException>(string message = null,
                                                               object[] args = null,
                                                               params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            if (string.IsNullOrEmpty(message))
                message = string.Empty;

            var tmp = new ConcurrentList<object> {message};

            if (args != null)
                tmp.AddRange(args);

            return GenerateException(
                                     () => (TException)Activator.CreateInstance(typeof(TException), tmp.ToArray()),
                                     data);
        }

        public static TException GenerateException<TException>(Func<TException> exceptionFactory,
                                                               params KeyValuePair<string, object>[] data)
            where TException : Exception, new()
        {
            if (exceptionFactory == null)
                throw new ArgumentNullException(nameof(exceptionFactory), "No exception factory was specified.");

            var exceptionToThrow = exceptionFactory();
            if (exceptionToThrow == null)
                throw new InvalidOperationException("An exception could not be generated through the given exception factory");

            // Add any additional content that the caller requires to exist in the Exception data.
            exceptionToThrow.Data.AddRange(data);

            // Add a time-stamp for when the exception was thrown.
            exceptionToThrow.Data.AddRange(
                                           new KeyValuePair<string, string>("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

            return exceptionToThrow;
        }

        private static Func<TException> ArgumentExceptionFactory<TException>(string argumentName, string message = null)
            where TException : Exception, new()
        {
            var type = typeof(TException);
            if (type == typeof(ArgumentException))
            {
                return () =>
                           new ArgumentException(message, argumentName) as TException;
            }

            if (type.IsSubclassOf(typeof(ArgumentException)))
            {
                if (TryGetFactoryWithTwoStringArguments(out var two))
                    return () => two(argumentName, message);

                if (TryGetFactoryWithOneStringArgument(out var one))
                    return () => one(argumentName);
            }
            else if (TryGetFactoryWithOneStringArgument(out var one)) return () => one(message);

            if (TryGetFactoryWithNoArguments(out var none))
                return () => none();
            return () =>
                   {
                       var x = new ArgumentException(message, argumentName);
                       throw new ArgumentException($"An instance of {type} cannot be initialized.", x);
                   };

            bool TryGetFactoryWithTwoStringArguments(out Func<string, string, TException> factory)
            {
                var ctor = type.GetConstructor(new[] {typeof(string), typeof(string)});
                if (ctor != null)
                {
                    var args = new[]
                               {
                                   Expression.Parameter(typeof(string), "arg1"),
                                   Expression.Parameter(typeof(string), "arg2")
                               };

                    factory = Expression.Lambda<Func<string, string, TException>>(
                                                                                  Expression.New(ctor, args),
                                                                                  args)
                                        .Compile();

                    return true;
                }

                factory = null;
                return false;
            }

            bool TryGetFactoryWithOneStringArgument(out Func<string, TException> factory)
            {
                var ctor = type.GetConstructor(new[] {typeof(string)});
                if (ctor != null)
                {
                    var arg = Expression.Parameter(typeof(string), "message");
                    factory = Expression.Lambda<Func<string, TException>>(
                                                                          Expression.New(ctor, arg),
                                                                          arg)
                                        .Compile();

                    return true;
                }

                factory = null;
                return false;
            }

            bool TryGetFactoryWithNoArguments(out Func<TException> factory)
            {
                var ctor = type.GetConstructor(new Type[] { });
                if (ctor != null)
                {
                    factory = Expression.Lambda<Func<TException>>(Expression.New(ctor)).Compile();
                    return true;
                }

                factory = null;
                return false;
            }
        }
        #endregion
    }
}