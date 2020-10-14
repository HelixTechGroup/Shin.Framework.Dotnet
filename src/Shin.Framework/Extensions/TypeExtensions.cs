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
        public static bool ContainsType(this IEnumerable<Type> test, Type t)
        {
            return test.Any(type => type.ContainsType(t));
        }

        public static bool ContainsType<T>(this IEnumerable<Type> test)
        {
            return test.ContainsType(typeof(T));
        }

        public static bool ContainsType(this Type test, Type t)
        {
            var t1 = test.IsAssignableFrom(t);
            var t2 = t.IsInstanceOfType(test);
            var t3 = test.ContainsInterface(t);
            return t1 || t2 || t3;
        }

        public static bool ContainsType<T>(this Type test)
        {
            return test.ContainsType(typeof(T));
        }

        public static bool ContainsType(this object test, Type t)
        {
            var t1 = test.GetType().IsAssignableFrom(t);
            var t2 = t.IsInstanceOfType(test);
            var t3 = test.ContainsInterface(t);
            return t1 || t2 || t3;
        }

        public static bool ContainsType<T>(this object test)
        {
            return test.ContainsType(typeof(T));
        }

        public static bool ContainsInterface(this Type type, Type t)
        {
            var res = false;
            if (!t.IsInterface)
                return res;

            //res = t.IsInstanceOfType(test);
            //var bType = test.GetType().BaseType;
            //if (bType == null)
            //    return res;

            var derived = type;
            do
            {
                res = derived == t || derived.GetInterfaces().Contains(t);
                //derived.IsSubclassOf(t);
                if (res)
                    return true;
                derived = derived.BaseType;
                if (derived != null)
                {
                    res = derived.ContainsInterface(t);
                }

                if (res)
                    return true;
            } while (derived != null);

            //foreach (var i in bType.GetInterfaces())
            //{
            //    if (i != t && !i.IsInstanceOfType(t)) 
            //        continue;

            //    res = true;
            //    break;
            //}

            return res;
            //return allInterfaces != null;//.Any(i => i == t);
        }

        public static bool ContainsInterface(this object test, Type t)
        {
            var type = test.GetType();
            return type.ContainsInterface(t);
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