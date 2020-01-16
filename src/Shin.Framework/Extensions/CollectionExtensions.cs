#region Usings
using System;
using System.Collections.Generic;
#endregion

namespace Shin.Framework.Extensions
{
    public static class CollectionExtensions
    {
        #region Methods
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var each in items)
                collection.Add(each);

            return collection;
        }
        #endregion
    }
}