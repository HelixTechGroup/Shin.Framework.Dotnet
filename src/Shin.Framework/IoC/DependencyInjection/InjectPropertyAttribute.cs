#region Usings
using System;
#endregion

namespace Shin.IoC.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectPropertyAttribute : Attribute { }
}