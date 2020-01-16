#region Usings
#endregion

#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion

namespace Shin.Framework.Extensions
{
    public static class AssemblyExtionsions
    {
        #region Methods
        //public static T GetAssemblyAttribute<T>(this Assembly assembly) where T : Attribute
        //{
        //    var attributes = assembly.GetCustomAttributes(typeof(T), false);
        //    return attributes.Length == 0 ? null : attributes.OfType<T>().SingleOrDefault();
        //}

        //public static T GetAssemblyAttribute<T>(this Assembly[] assemblies) where T : Attribute
        //{
        //    return (from assembly in assemblies from attribute in assembly.GetCustomAttributes(typeof(T), false) select attribute).OfType<T>()
        //                                                                                                                          .SingleOrDefault();
        //}

        public static IEnumerable<T> GetAssemblyAttribute<T>(this Assembly assembly) where T : Attribute
        {
            return assembly.GetCustomAttributes(typeof(T), false).OfType<T>();
        }

        public static IEnumerable<T> GetAssemblyAttribute<T>(this IEnumerable<Assembly> assemblies) where T : Attribute
        {
            return (from assembly in assemblies from attribute in assembly.GetCustomAttributes(typeof(T), false) select attribute).OfType<T>();
        }
        #endregion
    }
}