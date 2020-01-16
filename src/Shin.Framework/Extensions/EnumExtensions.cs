#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
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
        #endregion
    }
}