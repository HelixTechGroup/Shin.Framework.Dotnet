#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.Exceptions;
#endregion

namespace Shin.Framework.Extensions
{
    public static class EnumExtensions
    {
        #region Methods
        public static IEnumerable<T> GetEnumValues<T>() where T : struct
        {
            var currentEnum = typeof(T);
            if (currentEnum.IsEnum)
            {
                var fields = currentEnum.GetFields(BindingFlags.Static | BindingFlags.Public);
                foreach (var f in fields)
                    yield return (T)f.GetValue(null);
            }
            else
                throw new TypeArgumentException("The argument must of type Enum or of a type derived from Enum");
        }

        public static string GetEnumDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static T GetEnumFromDescription<T>(this string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new TypeArgumentException();

            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Enum was not found matching the description.", nameof(description));

            // or return default(T);
        }

        public static T EnumFromString<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static bool HasValue(this Enum value, Enum test)
        {
            return ReferenceEquals(value, test);
        }

        public static bool ContainsFlag(this Enum value, Enum test)
        {
            var intValue = Convert.ToUInt64(value);
            var intTest = Convert.ToUInt64(test);
            return ((intValue & intTest) == intTest);
        }

        public static bool[] ContainsFlags(this Enum value, params Enum[] test)
        {
            var found = new ConcurrentList<bool>();

            foreach (var t in test)
                found.Add(value.HasFlag(t));

            return found.ToArray();
        }

        //public static void SelectAndExecute<TParam>(this Enum value, Enum test, params Func<TParam, bool>[] actions)
        //{
        //    //foreach (var et in value.GetType().GetEnumValues())
        //    //{
        //    if ((!value.HasFlag(test)))
        //        return;

        //    _ = actions?.Any(a => a != null && !a.Invoke(test));
        //    //}

        //}
        #endregion
    }
}