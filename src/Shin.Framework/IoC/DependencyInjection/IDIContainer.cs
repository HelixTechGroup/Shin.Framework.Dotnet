#region Usings
using System;
using System.Collections.Generic;
using sysComponent = System.ComponentModel;
#endregion

namespace Shin.Framework.IoC.DependencyInjection
{
    public interface IDIContainer : IDispose
    {
        #region Properties
        Guid Id { get; }

        IEnumerable<Type> RegisteredTypes { get; }

        IDIRootContainer Root { get; }
        #endregion

        #region Methods
        void Load(params IBindings[] bindings);

        void Unload(params IBindings[] bindings);

        void Register<T>(T value, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register(object value, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register<T>(bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register(Type T, object value, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register(Type T, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register<T>(Type C, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register<T, C>(C value, bool asSingleton = true, string key = null, bool overrideExisting = false) where C : class, T;

        void Register<T, C>(bool asSingleton = true, string key = null, bool overrideExisting = false) where C : class, T;

        void Register(Type T, Type C, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Unregister<T>(string key = null);

        void Unregister(Type T, string key = null);

        void Unregister(object value);

        void UnregisterAll(Type T);

        void UnregisterAll<T>();

        T Resolve<T>(string key = null, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters);

        object Resolve(Type T, string key = null, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters);

        IEnumerable<T> ResolveAll<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default);

        IEnumerable<object> ResolveAll(Type T, DIResolutionStrategy strategy = DIResolutionStrategy.Default);

        bool TryResolve<T>(out T result,
                           string key = null,
                           DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                           params object[] parameters);

        bool TryResolve(Type T,
                        out object result,
                        string key = null,
                        DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                        params object[] parameters);

        void Release();

        Type ResolveType(string key);

        string ResolveKey(Type type);

        string ResolveKey<T>();

        bool IsKeyRegistered(string key, DIResolutionStrategy strategy = DIResolutionStrategy.Default);

        bool IsTypeRegistered<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default);

        bool IsTypeRegistered(Type T, DIResolutionStrategy strategy = DIResolutionStrategy.Default);
        #endregion
    }
}