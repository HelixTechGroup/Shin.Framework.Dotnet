#region Usings
using System;
using System.Reflection;
#endregion

namespace Shield.Framework.IoC.Native.DependencyInjection
{
    internal sealed class ConstructorInvokeInfo
    {
        #region Members
        internal readonly ConstructorInfo Constructor;
        internal readonly ParameterInfo[] ParameterInfos;

        Func<object[], object> constructorFunc;
        #endregion

        #region Properties
        internal Func<object[], object> ConstructorFunc
        {
            get { return constructorFunc ?? (constructorFunc = ReflectionCompiler.CreateFunc(Constructor)); }
        }
        #endregion

        internal ConstructorInvokeInfo(ConstructorInfo constructor)
        {
            Constructor = constructor;
            ParameterInfos = constructor.GetParameters();
        }
    }
}