﻿#region Usings
#endregion

#region Usings
using System;
using System.Collections.Generic;
using System.IO;
#endregion

namespace Shin.Framework.Exceptions
{
    internal static partial class ExceptionProvider
    {
        #region Methods
        public static ArgumentNullException ArgumentNullException(string paramName,
                                                                  string message = null,
                                                                  params KeyValuePair<string, object>[] data)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "An exception occurred.";

            return GenerateException(
                                     () => (ArgumentNullException)Activator.CreateInstance(typeof(ArgumentNullException), paramName, message),
                                     data);
        }

        public static ArgumentException ArgumentException(string paramName,
                                                          string message = null,
                                                          params KeyValuePair<string, object>[] data)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "An exception occurred.";

            return GenerateException(
                                     () => (ArgumentException)Activator.CreateInstance(typeof(ArgumentException), message, paramName),
                                     data);
        }

        public static ArgumentOutOfRangeException ArgumentOutOfRangeException(string paramName,
                                                                              object actualValue = null,
                                                                              string message = null,
                                                                              params KeyValuePair<string, object>[] data)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "An exception occurred.";

            return GenerateException(
                                     () => (ArgumentOutOfRangeException)Activator.CreateInstance(typeof(ArgumentOutOfRangeException),
                                                                                                 message,
                                                                                                 paramName,
                                                                                                 actualValue),
                                     data);
        }

        public static InvalidOperationException InvalidOperationException(string message = null,
                                                                          params KeyValuePair<string, object>[] data)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "An exception occurred.";

            return GenerateException(
                                     () => (InvalidOperationException)Activator.CreateInstance(typeof(InvalidOperationException), message),
                                     data);
        }

        public static UnauthorizedAccessException UnauthorizedAccessException(string message = null,
                                                                              params KeyValuePair<string, object>[] data)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "An exception occurred.";

            return GenerateException(
                                     () => (UnauthorizedAccessException)Activator.CreateInstance(typeof(UnauthorizedAccessException), message),
                                     data);
        }

        public static IOException IOException(string message = null,
                                              params KeyValuePair<string, object>[] data)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "An exception occurred.";

            return GenerateException(
                                     () => (IOException)Activator.CreateInstance(typeof(IOException), message),
                                     data);
        }
        #endregion
    }
}