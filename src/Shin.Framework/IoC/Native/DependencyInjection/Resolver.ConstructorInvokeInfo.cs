#region Usings
#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.Extensions;
using Shin.Framework.IoC.Native.DependencyInjection.Exceptions;

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    internal partial class Resolver
    {
        protected sealed class ConstructorInvokeInfo
        {
            #region Members
            internal readonly ConstructorInfo Constructor;
            internal readonly ParameterInfo[] ParameterInfos;

            private Func<object[], object> m_constructorFunc;
            private Expression m_expression;
            #endregion

            //internal Expression ConstructorExpression(params object[] parameters)
            //{

            //}

            //internal Func<object[], object> ConstructorFunc
            //{
            //    get { return m_constructorFunc ??= ReflectionCompiler.CreateFunc(Constructor); }
            //}

            internal ConstructorInvokeInfo(ConstructorInfo constructor)
            {
                Constructor = constructor;
                ParameterInfos = constructor.GetParameters();
            }

            internal object Invoke(params object[] parameters)
            {
                return ReflectionCompiler.CreateDelegate(Constructor)?.DynamicInvoke(parameters);
            }
        }
    }
}