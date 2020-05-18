#region Usings
using System;
using System.Collections.Generic;
#endregion

namespace Shin.Framework.IoC.DependencyInjection
{
    public interface IContainer : IDispose
    {
        #region Methods
        IContainer CreateChildContainer();

        void Load(params IBindings[] bindings);

        void Unload(params IBindings[] bindings);

        void Register<T>(T value, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register(object value, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register<T>(bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register(Type T, object value, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register(Type T, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Register<T, C>(C value, bool asSingleton = true, string key = null, bool overrideExisting = false) where C : class, T;

        void Register<T, C>(bool asSingleton = true, string key = null, bool overrideExisting = false) where C : class, T;

        void Register(Type T, Type C, bool asSingleton = true, string key = null, bool overrideExisting = false);

        void Unregister<T>(string key = null);

        void Unregister(Type T, string key = null);

        void Unregister(object value);

        void UnregisterAll(Type T);

        void UnregisterAll<T>();

        T Resolve<T>(string key = null);

        object Resolve(Type T, string key = null);

        IEnumerable<T> ResolveAll<T>();

        IEnumerable<object> ResolveAll(Type T);

        T TryResolve<T>(string key = null);

        object TryResolve(Type T, string key = null);

        void Release();

        bool IsRegistered<T>();

        bool IsRegistered(Type T);
        #endregion
    }
}