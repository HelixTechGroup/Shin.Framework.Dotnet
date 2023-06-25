#region Usings
using System;
using sysComponent = System.ComponentModel;
#endregion

namespace Shin.Framework.IoC.DependencyInjection
{
    public partial interface IDIContainer
    {
        #region Methods
        void Register<T>(Func<T> createInstanceFunc,
                         bool asSingleton = true,
                         string key = null,
                         bool overrideExisting = false);

        void Register<T>(Func<object[], T> createInstanceFunc,
                         bool asSingleton = true,
                         string key = null,
                         bool overrideExisting = false);

        void Register<T>(T value,
                         bool asSingleton = true,
                         string key = null,
                         bool overrideExisting = false);

        void Register(object value,
                      bool asSingleton = true,
                      string key = null,
                      bool overrideExisting = false);

        void Register<T>(bool asSingleton = true,
                         string key = null,
                         bool overrideExisting = false);

        void Register(Type T,
                      object value,
                      bool asSingleton = true,
                      string key = null,
                      bool overrideExisting = false);

        void Register(Type T,
                      bool asSingleton = true,
                      string key = null,
                      bool overrideExisting = false);

        void Register(Type T,
                      Func<object> createInstanceFunc,
                      bool asSingleton = true,
                      string key = null,
                      bool overrideExisting = false);

        void Register(Type T,
                      Func<object[], object> createInstanceFunc,
                      bool asSingleton = true,
                      string key = null,
                      bool overrideExisting = false);

        void Register<T>(Type C,
                         bool asSingleton = true,
                         string key = null,
                         bool overrideExisting = false);

        void Register<T, C>(C value,
                            bool asSingleton = true,
                            string key = null,
                            bool overrideExisting = false)
            where C : class, T;

        void Register<T, C>(bool asSingleton = true,
                            string key = null,
                            bool overrideExisting = false)
            where C : class, T;

        void Register<T>(Type C,
                         Func<T> createInstanceFunc,
                         bool asSingleton = true,
                         string key = null,
                         bool overrideExisting = false);

        void Register<T>(Type C,
                         Func<object[], T> createInstanceFunc,
                         bool asSingleton = true,
                         string key = null,
                         bool overrideExisting = false);

        void Register<T, C>(Func<C> createInstanceFunc,
                            bool asSingleton = true,
                            string key = null,
                            bool overrideExisting = false)
            where C : class, T;


        void Register<T, C>(Func<object[], C> createInstanceFunc,
                            bool asSingleton = true,
                            string key = null,
                            bool overrideExisting = false)
            where C : class, T;

        void Register(Type T,
                      Type C,
                      bool asSingleton = true,
                      string key = null,
                      bool overrideExisting = false);

        void Register(Type T,
                      Type C,
                      Func<object> createInstanceFunc,
                      bool asSingleton = true,
                      string key = null,
                      bool overrideExisting = false);

        void Register(Type T,
                      Type C,
                      Func<object[], object> createInstanceFunc,
                      bool asSingleton = true,
                      string key = null,
                      bool overrideExisting = false);

        void Unregister<T>(string key = null);

        void Unregister(Type T,
                        string key = null);

        void Unregister(object value);

        void UnregisterAll(Type T);

        void UnregisterAll<T>();

        bool IsKeyRegistered(string key,
                             DIResolutionStrategy strategy = DIResolutionStrategy.Default);

        bool IsTypeRegistered<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default);

        bool IsTypeRegistered(Type T,
                              DIResolutionStrategy strategy = DIResolutionStrategy.Default);
        #endregion
    }
}