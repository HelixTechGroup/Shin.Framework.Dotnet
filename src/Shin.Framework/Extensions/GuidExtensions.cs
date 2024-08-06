using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace Shin.Extensions
{
    public static class GuidExtensions
    {
        public static IEnumerable<Type> ToType(this ICollection<Guid> collection)
        {
            return collection.Select(Type.GetTypeFromCLSID);
        }

        public static Type ToType(this Guid guid)
        {
            return Type.GetTypeFromCLSID(guid, true);
        }
    }
}