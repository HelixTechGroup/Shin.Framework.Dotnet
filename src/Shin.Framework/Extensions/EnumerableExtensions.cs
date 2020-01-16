#region Usings
#endregion

#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Shin.Framework.Extensions
{
    public static class EnumerableExtensions
    {
        #region Methods
        public static bool IsEmpty(this IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            try
            {
                return !enumerator.MoveNext();
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        public static IEnumerable<T> Slice<T>(this IEnumerable<T> e,
                                              int beginIndex,
                                              int endIndex)
        {
            var enumerable = e as T[] ?? e.ToArray();
            if (!enumerable.Any())
                return enumerable;
            return beginIndex >= 0 ? enumerable.Skip(beginIndex) : enumerable.Skip(enumerable.Count() - beginIndex);
        }
        #endregion
    }
}