#region Usings
#endregion

#region Usings
using System;
using Shin.Framework.Exceptions;
#endregion

namespace Shin.Framework.Extensions
{
    internal static class ObjectExtensions
    {
        #region Methods
        internal static void ThrowIfNull<T, TException>(this T obj, string message = null) where TException : Exception, new()
        {
            if (obj == null)
                throw ExceptionProvider.GenerateException<TException>(message);
        }

        internal static void ThrowIfNull<T>(this T obj, string message = null)
        {
            obj.ThrowIfNull<T, NullReferenceException>(message);
        }
        #endregion
    }
}