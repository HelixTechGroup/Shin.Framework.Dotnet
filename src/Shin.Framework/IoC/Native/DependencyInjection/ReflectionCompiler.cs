#region Usings
#endregion

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    internal static class ReflectionCompiler
    {
        #region Method Callers
        public static Delegate CreateDelegate(ConstructorInfo method)
        {
            var pInfos = method.GetParameters();
            return CreateCompiledExpression(method, pInfos);
        }

        public static Action<object, object[]> CreateAction(MethodInfo method, params object[] parameters)
        {
            var pInfos = method.GetParameters();
            var parametersLength = pInfos.Length;

            var compiledExpression = CreateCompiledExpression(method, parametersLength, pInfos);
            
            var voidReturnType = method.ReturnType == typeof(void);

            Action<object, object[]> result = null;

            //if (voidReturnType)
            //{
            //    switch (parametersLength)
            //    {
            //        case 0:
            //        {
            //            var action = (Action<object>)compiledExpression;
            //            result = (o, args) => { action(o); };
            //            break;
            //        }
            //        case 1:
            //        {
            //            var action = (Action<object, object>)compiledExpression;
            //            result = (o, args) => { action(o, args[0]); };
            //            break;
            //        }
            //        case 2:
            //        {
            //            var action = (Action<object, object, object>)compiledExpression;
            //            result = (o, args) => { action(o, args[0], args[1]); };
            //            break;
            //        }
            //        case 3:
            //        {
            //            var action = (Action<object, object, object, object>)compiledExpression;
            //            result = (o, args) => { action(o, args[0], args[1], args[2]); };
            //            break;
            //        }
            //        case 4:
            //        {
            //            var action = (Action<object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => { action(o, args[0], args[1], args[2], args[3]); };
            //            break;
            //        }
            //        case 5:
            //        {
            //            var action = (Action<object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => { action(o, args[0], args[1], args[2], args[3], args[4]); };
            //            break;
            //        }
            //        case 6:
            //        {
            //            var action = (Action<object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => { action(o, args[0], args[1], args[2], args[3], args[4], args[5]); };
            //            break;
            //        }
            //        case 7:
            //        {
            //            var action = (Action<object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => { action(o, args[0], args[1], args[2], args[3], args[4], args[5], args[6]); };
            //            break;
            //        }
            //        case 8:
            //        {
            //            var action = (Action<object, object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => { action(o, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]); };
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    switch (parametersLength)
            //    {
            //        case 0:
            //        {
            //            var func = (Func<object, object>)compiledExpression;
            //            result = (o, args) => func(o);
            //            break;
            //        }
            //        case 1:
            //        {
            //            var func = (Func<object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0]);
            //            break;
            //        }
            //        case 2:
            //        {
            //            var func = (Func<object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1]);
            //            break;
            //        }
            //        case 3:
            //        {
            //            var func = (Func<object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2]);
            //            break;
            //        }
            //        case 4:
            //        {
            //            var func = (Func<object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3]);
            //            break;
            //        }
            //        case 5:
            //        {
            //            var func = (Func<object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3], args[4]);
            //            break;
            //        }
            //        case 6:
            //        {
            //            var func = (Func<object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3], args[4], args[5]);
            //            break;
            //        }
            //        case 7:
            //        {
            //            var func = (Func<object, object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
            //            break;
            //        }
            //        case 8:
            //        {
            //            var func = (Func<object, object, object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            //            break;
            //        }
            //    }
            //}

            return result;
        }

        public static Func<object, object[], object> CreateFunc(MethodInfo method)
        {
            var pInfos = method.GetParameters();
            var parametersLength = pInfos.Length;

            var compiledExpression = CreateCompiledExpression(method, parametersLength, pInfos);

            Func<object, object[], object> result = null;

            var voidMethod = method.ReturnType == typeof(void);

            //if (voidMethod)
            //{
            //    switch (parametersLength)
            //    {
            //        case 0:
            //        {
            //            var action = (Action<object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o);
            //                         return null;
            //                     };
            //            break;
            //        }
            //        case 1:
            //        {
            //            var action = (Action<object, object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o, args[0]);
            //                         return null;
            //                     };
            //            break;
            //        }
            //        case 2:
            //        {
            //            var action = (Action<object, object, object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o, args[0], args[1]);
            //                         return null;
            //                     };
            //            break;
            //        }
            //        case 3:
            //        {
            //            var action = (Action<object, object, object, object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o, args[0], args[1], args[2]);
            //                         return null;
            //                     };
            //            break;
            //        }
            //        case 4:
            //        {
            //            var action = (Action<object, object, object, object, object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o, args[0], args[1], args[2], args[3]);
            //                         return null;
            //                     };
            //            break;
            //        }
            //        case 5:
            //        {
            //            var action = (Action<object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o, args[0], args[1], args[2], args[3], args[4]);
            //                         return null;
            //                     };
            //            break;
            //        }
            //        case 6:
            //        {
            //            var action = (Action<object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o, args[0], args[1], args[2], args[3], args[4], args[5]);
            //                         return null;
            //                     };
            //            break;
            //        }
            //        case 7:
            //        {
            //            var action = (Action<object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
            //                         return null;
            //                     };
            //            break;
            //        }
            //        case 8:
            //        {
            //            var action = (Action<object, object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) =>
            //                     {
            //                         action(o, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            //                         return null;
            //                     };
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    switch (parametersLength)
            //    {
            //        case 0:
            //        {
            //            var func = (Func<object, object>)compiledExpression;
            //            result = (o, args) => func(o);
            //            break;
            //        }
            //        case 1:
            //        {
            //            var func = (Func<object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0]);
            //            break;
            //        }
            //        case 2:
            //        {
            //            var func = (Func<object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1]);
            //            break;
            //        }
            //        case 3:
            //        {
            //            var func = (Func<object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2]);
            //            break;
            //        }
            //        case 4:
            //        {
            //            var func = (Func<object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3]);
            //            break;
            //        }
            //        case 5:
            //        {
            //            var func = (Func<object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3], args[4]);
            //            break;
            //        }
            //        case 6:
            //        {
            //            var func = (Func<object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3], args[4], args[5]);
            //            break;
            //        }
            //        case 7:
            //        {
            //            var func = (Func<object, object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
            //            break;
            //        }
            //        case 8:
            //        {
            //            var func = (Func<object, object, object, object, object, object, object, object, object, object>)compiledExpression;
            //            result = (o, args) => func(o, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            //            break;
            //        }
            //    }
            //}

            return result;
        }


        public static Func<object[], object> CreateFunc(ConstructorInfo method, params object[] parameters)
        {
            Func<object[], object> result = null;
            var pInfos = method.GetParameters();

            var compiledExpression = CreateCompiledExpression(method, pInfos);

            var parametersLength = parameters.Length;

            //switch (parametersLength)
            //{
            //    case 0:
            //    {
            //        var func = (Func<object>)compiledExpression;
            //        result = o => func();
            //        break;
            //    }
            //    case 1:
            //    {
            //        var func = (Func<object, object>)compiledExpression;
            //        result = args => 
            //        { 
            //            if (args is null || args[0] is null) 
            //                return null; 
            //            return func(args[0]); 
            //        };
            //        break;
            //    }
            //    case 2:
            //    {
            //        var func = (Func<object, object, object>)compiledExpression;
            //        result = args => func(args[0], args[1]);
            //        break;
            //    }
            //    case 3:
            //    {
            //        var func = (Func<object, object, object, object>)compiledExpression;
            //        result = args => func(args[0], args[1], args[2]);
            //        break;
            //    }
            //    case 4:
            //    {
            //        var func = (Func<object, object, object, object, object>)compiledExpression;
            //        result = args => func(args[0], args[1], args[2], args[3]);
            //        break;
            //    }
            //    case 5:
            //    {
            //        var func = (Func<object, object, object, object, object, object>)compiledExpression;
            //        result = args => func(args[0], args[1], args[2], args[3], args[4]);
            //        break;
            //    }
            //    case 6:
            //    {
            //        var func = (Func<object, object, object, object, object, object, object>)compiledExpression;
            //        result = args => func(args[0], args[1], args[2], args[3], args[4], args[5]);
            //        break;
            //    }
            //    case 7:
            //    {
            //        var func = (Func<object, object, object, object, object, object, object, object>)compiledExpression;
            //        result = args => func(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
            //        break;
            //    }
            //    case 8:
            //    {
            //        var func = (Func<object, object, object, object, object, object, object, object, object>)compiledExpression;
            //        result = args => func(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            //        break;
            //    }
            //}

            return result;
        }

        static Delegate CreateCompiledExpression(ConstructorInfo method, ParameterInfo[] parameterInfos)
        {
            var parametersLength = parameterInfos.Length;
            var parameterExpressions = new ParameterExpression[parametersLength];
            //var paramTypes = new Expression[parametersLength];

            for (var i = 0; i < parametersLength; i++)
            {
                var parameterInfo = parameterInfos[i];
                var pEx = Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name);
                parameterExpressions[i] = pEx;

                //var expression = Expression.Convert(objectParameter, parameterInfo.ParameterType);
                //paramTypes[i] = pEx;
            }

            var newExpression = Expression.New(method, parameterExpressions);
            var lambda = Expression.Lambda(newExpression, parameterExpressions);
            var compiledExpression = lambda.Compile();
            return compiledExpression;
        }

        static Delegate CreateCompiledExpression(MethodInfo method, int parametersLength, ParameterInfo[] pInfos)
        {
            var parameterExpressions = new ParameterExpression[parametersLength + 1];
            //parameterExpressions[0] = Expression.Parameter(typeof(object), "obj");

            //var paramTypes = new Expression[parametersLength];

            for (var i = 0; i < parametersLength; i++)
            {
                var info = pInfos[i];
                var parameter = Expression.Parameter(info.ParameterType, info.Name);
                /* Skip the first item as that is the object 
                 * on which the method is called. */
                parameterExpressions[i + 1] = parameter;

                
                //string typeName = info.ParameterType.FullName.Replace("&", string.Empty);
                //var type = Type.GetType(typeName);
                //var expression = Expression.Convert(parameter, info.ParameterType);
                //paramTypes[i] = expression;
            }

            var instanceExpression = Expression.Convert(parameterExpressions[0], method.DeclaringType);

            var voidMethod = method.ReturnType == typeof(void);

            Expression callExpression;

            if (voidMethod)
            {
                callExpression = Expression.Call(instanceExpression, method, parameterExpressions);
            }
            else
            {
                callExpression = Expression.Convert(Expression.Call(instanceExpression, method, parameterExpressions), typeof(object));
            }

            var lambda = Expression.Lambda(callExpression, parameterExpressions);
            var compiledExpression = lambda.Compile();
            return compiledExpression;
        }
        #endregion

        #region Property Accessors
        public static Action<object, object> CreateSetter(PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);

            if (setMethod == null || setMethod.GetParameters().Length != 1)
            {
                throw new ArgumentException($"Property {property.DeclaringType}.{property.Name} has no setter or parameters Length not equal to 1.");
            }

            var obj = Expression.Parameter(typeof(object), "o");
            var value = Expression.Parameter(typeof(object));

            var expr =
                Expression.Lambda<Action<object, object>>(
                                                          Expression.Call(
                                                                          Expression.Convert(obj, setMethod.DeclaringType),
                                                                          setMethod,
                                                                          Expression.Convert(value, setMethod.GetParameters()[0].ParameterType)),
                                                          obj,
                                                          value);

            return expr.Compile();
        }

        public static Func<object, object> CreateGetter(PropertyInfo property)
        {
            var getMethod = property.GetGetMethod(true);

            if (getMethod == null || getMethod.GetParameters().Length != 0)
            {
                throw new ArgumentException($"Property {property.DeclaringType}.{property.Name} has no getter or parameters Length not equal to 0.");
            }

            var returnType = getMethod.ReturnType;

#if NETFX_CORE
			if (!returnType.GetTypeInfo().IsValueType)
			{
				return Compile<object>(getMethod);
			}
#else
            if (!returnType.IsValueType)
            {
                return Compile<object>(getMethod);
            }
#endif

            var method = typeof(ReflectionCompiler).GetMethod(nameof(CoerceCompiled), BindingFlags.Static | BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(returnType);

            var compiled = (Func<object, object>)genericMethod.Invoke(null, new object[] {getMethod});
            return compiled;
        }

        static Func<object, object> CoerceCompiled<T>(MethodInfo getMethod)
        {
            var compiled = Compile<T>(getMethod);
            Func<object, object> result = o => compiled(o);
            return result;
        }

        static Func<object, T> Compile<T>(MethodInfo getMethod)
        {
            var obj = Expression.Parameter(typeof(object), "o");

            var expr =
                Expression.Lambda<Func<object, T>>(
                                                   Expression.Call(
                                                                   Expression.Convert(obj, getMethod.DeclaringType),
                                                                   getMethod),
                                                   obj);

            return expr.Compile();
        }
        #endregion
    }
}