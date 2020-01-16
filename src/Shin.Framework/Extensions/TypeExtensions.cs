#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Shin.Framework.Extensions
{
    public static class TypeExtensions
    {
        #region Methods
        public static bool ContainsType(this object test, Type t)
        {
            return test.GetType() == t || test.GetType().IsSubclassOf(t);
        }

        public static bool ContainsType<T>(this object test)
        {
            return test.ContainsType(typeof(T));
        }

        public static bool ContainsInterface(this object test, Type t)
        {
            if (!t.IsInterface)
                return false;

            var allInterfaces = test.GetType().GetInterfaces();
            return allInterfaces.Any(i => i == t);
        }

        public static bool ContainsInterface<T>(this object test)
        {
            return test.ContainsInterface(typeof(T));
        }

        public static IEnumerable<Type> GetTopLevelInterfaces(this Type t)
        {
            var allInterfaces = t.GetInterfaces().Where(a => a.BaseType != null);

            if (t.BaseType == null)
                return allInterfaces;

            var interfaces = allInterfaces.ToArray();
            var selection = interfaces
                           .Where(x => !interfaces.Any(y => y.GetInterfaces().Contains(x)))
                           .Except(t.BaseType.GetInterfaces());

            return selection;
        }
        #endregion
    }
}