using System;
using System.Reflection;

namespace Shield.Framework.IoC.Native.DependencyInjection
{
    internal sealed class ConstructorInvokeInfo
    {
        internal readonly ParameterInfo[] ParameterInfos;

        Func<object[], object> constructorFunc;

        internal Func<object[], object> ConstructorFunc
        {
            get { return constructorFunc ?? (constructorFunc = ReflectionCompiler.CreateFunc(Constructor)); }
        }

        internal readonly ConstructorInfo Constructor;

        internal ConstructorInvokeInfo(ConstructorInfo constructor)
        {
            Constructor = constructor;
            ParameterInfos = constructor.GetParameters();
        }
    }
}
