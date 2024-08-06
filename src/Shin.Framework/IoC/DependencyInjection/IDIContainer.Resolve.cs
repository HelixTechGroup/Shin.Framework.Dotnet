#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Shin.IoC.DependencyInjection
{
    public partial interface IDIContainer
    {
        #region Methods
        T Resolve<T>(string key = null, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters);

        object Resolve(Type T, string key = null, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters);

        IEnumerable<T> ResolveAll<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters);

        IEnumerable<object> ResolveAll(Type T, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters);

        bool TryResolve<T>(out T result,
                           string key = null,
                           DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                           params object[] parameters);

        bool TryResolve(Type T,
                        out object result,
                        string key = null,
                        DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                        params object[] parameters);

        bool TryResolveAll(Type T, out IEnumerable result,
                              DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                              params object[] parameters);
        
        bool TryResolveAll<T>(out IEnumerable<T> result,
                              DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                              params object[] parameters);

        //Type ResolveType(string key);

        //string ResolveTypeId(Type type);

        //string ResolveKey<T>();
        #endregion
    }
}