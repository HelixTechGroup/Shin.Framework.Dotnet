#region Usings
using System;
#endregion

namespace Shin.Framework.IoC.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectPropertyAttribute : Attribute { }
}