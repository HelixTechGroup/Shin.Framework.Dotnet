using System;

namespace Shin.Framework.IoC.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class InjectConstructorAttribute : Attribute
    {
    }
}
