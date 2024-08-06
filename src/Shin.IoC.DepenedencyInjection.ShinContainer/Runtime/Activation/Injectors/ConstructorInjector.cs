#region Usings
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Shin.Collections.Concurrent;
using Shin.Extensions;
using Shin.IoC.DependencyInjection;
#endregion

namespace Shin.IoC.DependencyInjection.Runtime.Activation.Injectors
{
    public struct ConstructorInjector : IConstructorInjector
    {
        #region Members
        public readonly ConstructorInfo Constructor;
        private readonly Delegate m_constructorDelegate = null;
        private readonly IDIContainer m_container;

        //private Func<object[], object> m_constructorFunc = null;
        //private Expression m_expression = null;
        #endregion

        #region Properties
        public ParameterInfo[] ParameterInfos
        {
            get { return Constructor?.GetParameters(); }
        }

        public Type Type
        {
            get { return Constructor?.DeclaringType; }
        }
        #endregion

        //internal Expression ConstructorExpression(params object[] parameters)
        //{

        //}

        //internal Func<object[], object> ConstructorFunc
        //{
        //    get { return m_constructorFunc ??= ReflectionCompiler.CreateFunc(Constructor); }
        //}

        public ConstructorInjector(IDIContainer container, ConstructorInfo constructor)
        {
            m_container = container;
            Constructor = constructor;
            m_constructorDelegate = ExpressionCompiler.CreateDelegate(Constructor);
            //ParameterInfos = constructor.GetParameters();
        }

        #region Methods
        public ITypeInstance Inject(params object[] parameters)
        {
            var param = BuildParameterList(parameters);
            return new TypeResolver.TypeInstance(this, m_constructorDelegate?.DynamicInvoke(param));
        }

        private object[] BuildParameterList(params object[] parameters)
        {
            var parametersList = new ConcurrentList<object>();
            var constructorParameters = ParameterInfos;
            var currentMatch = constructorParameters.Length == parameters.Length;
            //var length = constructorParameters.Length;
            // var test = parameters.Length == length;
            // *test ? parameters : */ //new object[length];
            var ps = new ConcurrentList<object>(parameters);
            object parameter = null;

            foreach (var pi in constructorParameters)
            {
                //if (constructorParameters.Length == parameters.Length)
                //{
                //if (currentMatch)
                //{
                //    try
                //    {
                //        var tmp = parameters[pi.Position];
                //        if (pi.ParameterType == tmp.GetType())
                //            parameter = tmp;
                //        tmp = null;
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e);
                //        //parameter = ResolveAux(pi.ParameterType);
                //    }   
                //}
                //}

                //if (parameter == null)
                //{
                foreach (var p in ps)
                {
                    if (!p.GetType()
                          .ContainsType(pi.ParameterType))
                        continue;

                    parameter = p;
                    break;
                }

                ps.Remove(parameter);
                //}

#if NET5_0_OR_GREATER
                if (pi.HasDefaultValue) parameter ??= pi.DefaultValue;
#endif

                parameter ??= m_container.Resolve(pi.ParameterType);

                if (parameter is null) continue;

                parametersList.Add(parameter);
                parameter = null;
            }

            //Throw.If<IoCResolutionException>(!ps.IsEmpty());
            return parametersList.ToArray();
        }
        #endregion
    }
}