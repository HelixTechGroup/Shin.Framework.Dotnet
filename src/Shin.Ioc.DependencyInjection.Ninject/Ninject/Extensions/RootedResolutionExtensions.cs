#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Syntax;
using Ninject.Extensions;
using Shin.IoC.DependencyInjection.Ninject.Activation;
#endregion

namespace Shin.IoC.DependencyInjection.Ninject.Extensions
{
    public static class RootedResolutionExtensions
    {
        #region Methods
        public static IRequest CreateRootedRequest(this IResolutionRoot root,
                                                   Type service,
                                                   RootedResolutionStrategy strategy,
                                                   [AllowNull] Func<IBindingMetadata, bool> constraint,
                                                   [AllowNull] IReadOnlyList<IParameter> parameters,
                                                   bool isOptional,
                                                   bool isUnique)
        {
            if (root is not IKernelWithId p)
                return new RootedRequest(service,
                                     constraint,
                                     parameters,
                                     null,
                                     isOptional,
                                     isUnique,
                                     strategy);

            return new RootedRequest(service,
                                     constraint,
                                     parameters,
                                     null,
                                     isOptional,
                                     isUnique,
                                     p.Id,
                                     strategy);
        }

        public static T Get<T>(this IResolutionRoot root,
                               RootedResolutionStrategy strategy,
                               params IParameter[] parameters)
        {
            return Get<T>(root, strategy, null, parameters);
        }

        public static T Get<T>(this IResolutionRoot root,
                               string name,
                               RootedResolutionStrategy strategy,
                               params IParameter[] parameters)
        {
            return Get<T>(root,
                          strategy,
                          b => b.Name == name,
                          parameters);
        }

        public static T Get<T>(IResolutionRoot root,
                               RootedResolutionStrategy strategy,
                               Func<IBindingMetadata, bool> constraint,
                               IReadOnlyList<IParameter> parameters)
        {
            return (T) ResolveSingle(root,
                                     typeof(T),
                                     strategy,
                                     constraint,
                                     parameters,
                                     false,
                                     true);
        }

        public static object Get(this IResolutionRoot root,
                                 Type service,
                                 RootedResolutionStrategy strategy,
                                 params IParameter[] parameters)
        {
            return Get(root,
                                 service,
                                 strategy,
                                 constraint: null,
                                 parameters);
        }

        public static object Get(this IResolutionRoot root,
                                 Type service,
                                 RootedResolutionStrategy strategy,
                                 string name,
                                 params IParameter[] parameters)
        {
            return Get(root, service, strategy, b => b.Name == name, parameters);
        }

        public static object Get(this IResolutionRoot root,
                                 Type service,
                                 RootedResolutionStrategy strategy,
                                 [AllowNull] Func<IBindingMetadata, bool> constraint,
                                 [AllowNull] params IParameter[] parameters)
        {
            return ResolveSingle(root,
                                 service,
                                 strategy,
                                 constraint,
                                 parameters,
                                 false,
                                 true);
        }

        public static IEnumerable<object> GetAll(this IResolutionRoot root,
                                                 Type service,
                                                 RootedResolutionStrategy strategy,
                                                 params IParameter[] parameters)
        {
            return GetAll(root,
                                         service,
                                         strategy,
                          constraint: null,
                                         parameters);
        }

        public static IEnumerable<object> GetAll(this IResolutionRoot root,
                                                 Type service,
                                                 RootedResolutionStrategy strategy,
                                                 string name,
                                                 params IParameter[] parameters)
        {
            return GetAll(root,
                                         service,
                          strategy,
                                         b => b.Name == name,
                                         parameters);
        }

        public static IEnumerable<object> GetAll(this IResolutionRoot root,
                                                 Type service,
                                                 RootedResolutionStrategy strategy,
                                                 Func<IBindingMetadata, bool> constraint,
                                                 params IParameter[] parameters)
        {
            return GetResolutionIterator(root,
                                         service,
                          strategy,
                                         constraint,
                                         parameters,
                                             true,
                                                 false);
        }

        public static IEnumerable<T> GetAll<T>(this IResolutionRoot root,
                                               RootedResolutionStrategy strategy,
                                               Func<IBindingMetadata, bool> constraint,
                                               params IParameter[] parameters)
        {
            return GetResolutionIterator(root,
                                         typeof(T),
                                         strategy,
                                         constraint,
                                         parameters,
                                         true,
                                         false)
               .Cast<T>();
        }

        public static IEnumerable<T> GetAll<T>(this IResolutionRoot root,
                                               RootedResolutionStrategy strategy,
                                               string name,
                                               params IParameter[] parameters)
        {
            return GetAll<T>(root,
                             strategy,
                             constraint: b => b.Name == name,
                             parameters);
        }

        public static object TryGet(this IResolutionRoot root,
                                    Type service,
                                    RootedResolutionStrategy strategy,
                                    Func<IBindingMetadata, bool> constraint,
                                    params IParameter[] parameters)
        {
            return TryGet(() => ResolveSingle(root,
                                              service,
                                              strategy,
                                              constraint,
                                              parameters,
                                              true,
                                              true));
        }

        public static object TryGet(this IResolutionRoot root,
                                    Type service,
                                    RootedResolutionStrategy strategy,
                                    string name,
                                    params IParameter[] parameters)
        {
            return TryGet(() => ResolveSingle(root,
                                              service,
                                              strategy,
                                              b => b.Name == name,
                                              parameters,
                                              true,
                                              true));
        }

        public static object TryGet(this IResolutionRoot root,
                                    Type service,
                                    RootedResolutionStrategy strategy,
                                    params IParameter[] parameters)
        {
            return TryGet(() => ResolveSingle(root,
                                              service,
                                              strategy,
                                              null,
                                              parameters,
                                              true,
                                              true));
        }

        private static object ResolveSingle(IResolutionRoot root,
                                            Type service,
                                            RootedResolutionStrategy strategy,
                                            [AllowNull] Func<IBindingMetadata, bool> constraint,
                                            [AllowNull] IReadOnlyList<IParameter> parameters,
                                            bool isOptional,
                                            bool isUnique)
        {
            var request = root.CreateRootedRequest(service,
                                                   strategy,
                                                   constraint,
                                                   parameters,
                                                   isOptional,
                                                   isUnique);
            return ((IRootKernel) root).ResolveSingle(request);
        }

        private static IEnumerable<object> GetResolutionIterator(IResolutionRoot root,
                                                                 Type service,
                                                                 RootedResolutionStrategy strategy,
                                                                 Func<IBindingMetadata, bool> constraint,
                                                                 IReadOnlyList<IParameter> parameters,
                                                                 bool isOptional,
                                                                 bool isUnique)
        {
            var request = root.CreateRootedRequest(service,
                                                strategy,
                                                   constraint,
                                                   parameters,
                                                   isOptional,
                                                   isUnique);
            return root.Resolve(request);
        }

        private static T TryGet<T>(Func<object> resolver)
        {
            try
            {
                return (T) resolver();
            }
            catch (ActivationException)
            {
                return default(T);
            }
        }

        private static object TryGet(Func<object> resolver)
        {
            try
            {
                return resolver();
            }
            catch (ActivationException)
            {
                return null;
            }
        }
        #endregion
    }
}