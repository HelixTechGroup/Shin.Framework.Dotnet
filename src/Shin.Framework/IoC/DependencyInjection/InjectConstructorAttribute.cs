#region Usings
using System;
#endregion

namespace Shin.IoC.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class InjectAttribute : Attribute { }
}