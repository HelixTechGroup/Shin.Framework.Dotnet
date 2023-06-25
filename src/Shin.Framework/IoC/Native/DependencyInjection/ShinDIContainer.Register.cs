#region Usings
using System;
using System.Linq;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.Extensions;
using Shin.Framework.IoC.DependencyInjection;
using Shin.Framework.IoC.Native.DependencyInjection.Exceptions;
using Shin.Framework.Threading;
#endregion

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    public sealed partial class ShinDIContainer
    {
        #region Methods
        public void Register<T>(Func<T> createInstanceFunc,
                                bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            TypeCheck<T>(createInstanceFunc?.Method.ReturnType, out var regType);
            //var regType = createInstanceFunc?.Method.ReturnType;
            CreateResolver(typeof(T),
                           regType,
                           key,
                           asSingleton,
                           overrideExisting,
                           p => createInstanceFunc());
        }


        public void Register<T>(Func<object[], T> createInstanceFunc,
                                bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            TypeCheck<T>(createInstanceFunc?.Method.ReturnType, out var regType);
            //CreateResolver(createInstanceFunc?.Method.ReturnType,
            //               createInstanceFunc,
            //               key,
            //               asSingleton,
            //               overrideExisting);
            CreateResolver(typeof(T),
                           regType,
                           key,
                           asSingleton,
                           overrideExisting,
                           p => createInstanceFunc(p));
        }

        public void Register<T>(T value,
                                bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            TypeCheck<T>(value.GetType(), out var regType);
            //CreateResolver(regType,
            //               p => value,
            //               key,
            //               asSingleton,
            //               overrideExisting);
            CreateResolver(typeof(T),
                           regType,
                           key,
                           asSingleton,
                           overrideExisting,
                           instanceObject: value);
        }

        public void Register(object value,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            //CreateResolver(value.GetType(),
            //               () => value,
            //               key,
            //               asSingleton,
            //               overrideExisting);


            CreateResolver(value.GetType(),
                           value.GetType(),
                           key,
                           asSingleton,
                           overrideExisting,
                           instanceObject: value);
        }

        public void Register<T>(bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            CreateResolver(typeof(T),
                           typeof(T),
                           key,
                           asSingleton,
                           overrideExisting);
        }

        public void Register(Type T,
                             object value,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            CreateResolver(T,
                           value.GetType(),
                           key,
                           asSingleton,
                           overrideExisting,
                           instanceObject: value);
        }

        public void Register(Type T,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            CreateResolver(T,
                           T,
                           key,
                           asSingleton,
                           overrideExisting);
        }


        public void Register(Type T,
                             Func<object> createInstanceFunc,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            throw new NotImplementedException();
        }


        public void Register(Type T,
                             Func<object[], object> createInstanceFunc,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            TypeCheck(T, createInstanceFunc?.Method.ReturnType, out var regType);
            CreateResolver(T,
                           regType,
                           key,
                           asSingleton,
                           overrideExisting);
        }


        public void Register<T>(Type C,
                                bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            TypeCheck<T>(C, out var regType);
            CreateResolver<T>(regType, key, asSingleton, overrideExisting);
        }

        public void Register<T, C>(C value,
                                   bool asSingleton = true,
                                   string key = null,
                                   bool overrideExisting = false)
            where C : class, T
        {
            CreateResolver<T, C>(key, asSingleton, overrideExisting, instanceObject: value);
        }

        public void Register<T, C>(bool asSingleton = true,
                                   string key = null,
                                   bool overrideExisting = false)
            where C : class, T
        {
            CreateResolver<T, C>(key,
                                  asSingleton, 
                                  overrideExisting);
        }


        public void Register<T>(Type C,
                                Func<T> createInstanceFunc,
                                bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            throw new NotImplementedException();
        }


        public void Register<T>(Type C,
                                Func<object[], T> createInstanceFunc,
                                bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            CreateResolver<T>(C,
                              key,
                           asSingleton,
                           overrideExisting,
                              createInstanceFunc);
        }


        public void Register<T, C>(Func<C> createInstanceFunc,
                                   bool asSingleton = true,
                                   string key = null,
                                   bool overrideExisting = false)
            where C : class, T
        {
            throw new NotImplementedException();
        }


        public void Register<T, C>(Func<object[], C> createInstanceFunc,
                                   bool asSingleton = true,
                                   string key = null,
                                   bool overrideExisting = false)
            where C : class, T
        {
            TypeCheck<T, C>(out var regType);
            CreateResolver<T, C>(key, asSingleton, overrideExisting, createInstanceFunc);
        }

        public void Register(Type T,
                             Type C,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            CreateResolver(T,
                           C,
                           key,
                           asSingleton,
                           overrideExisting);
        }


        public void Register(Type T,
                             Type C,
                             Func<object> createInstanceFunc,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            throw new NotImplementedException();
        }


        public void Register(Type T,
                             Type C,
                             Func<object[], object> createInstanceFunc,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            CreateResolver(T,
                           C,
                           key,
                           asSingleton,
                           overrideExisting,
                           createInstanceFunc);
        }


        public void Unregister<T>(string key = null) { throw new NotImplementedException(); }

        public void Unregister(Type T,
                               string key = null)
        {
            throw new NotImplementedException();
        }


        public void Unregister(object value) { throw new NotImplementedException(); }


        public void UnregisterAll(Type T) { throw new NotImplementedException(); }


        public void UnregisterAll<T>() { throw new NotImplementedException(); }


        //public Type ResolveType(string key)
        //{
        //    GetTypeFromContainer(key, out var result);
        //    return result;
        //}

        public bool IsKeyRegistered(string key,
                                    DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            return GetTypeFromContainer(key, out var type);
        }


        public bool IsTypeRegistered<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            return IsTypeRegistered(typeof(T), strategy);
        }

        public bool IsTypeRegistered(Type T,
                                     DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            //m_lockSlim.TryEnter();
            var result = false;
            var selfOnly = strategy.HasFlag(DIResolutionStrategy.SelfOnly);

            if (!selfOnly)
            {
                if (strategy.HasFlag(DIResolutionStrategy.ChildrenOnly) ||
                    strategy.HasFlag(DIResolutionStrategy.ChildrenFirst))
                    result = ((IDIParentContainer) this).IsTypeRegisteredByChild(T);

                if (!result &&
                    (strategy.HasFlag(DIResolutionStrategy.ParentFirst) || strategy.HasFlag(DIResolutionStrategy.ParentOnly)))
                    result = ((IDIChildContainer) this).IsTypeRegisteredByParent(T);
            }

            if (!result &&
                (strategy.HasFlag(DIResolutionStrategy.IncludeSelf) || selfOnly))
            {
                result = m_typeDictionary.ContainsKey(T);

                if (!result                          &&
                    InterfaceCheck(T, out var cType) &&
                    cType is not null)
                {
                    foreach (var t in cType) result = m_typeDictionary.ContainsKey(t) /*|| m_interfaceDictionary.ContainsKey(T)*/;
                }
            }

            if (!result &&
                !selfOnly)
            {
                if (strategy.HasFlag(DIResolutionStrategy.ChildrenLast)) result = ((IDIParentContainer) this).IsTypeRegisteredByChild(T);

                if (!result &&
                    strategy.HasFlag(DIResolutionStrategy.ParentLast))
                    result = ((IDIChildContainer) this).IsTypeRegisteredByParent(T);
            }

            return result;
        }

        //private void CreateResolver(Type T,
        //                            Type C,
        //                            string key = null,
        //                            bool asSingleton = false,
        //                            bool overrideExisting = false,
        //                            Func<object> instanceFunc = null,
        //                            object instanceObject = null)
        //{
        //    CreateResolverCore(T,
        //                       C,
        //                       key,
        //                       asSingleton,
        //                       overrideExisting,
        //                       p => instanceFunc, 
        //                       instanceObject);
        //}

        private void CreateResolver(Type T,
                                    Type C,
                                    string key = null,
                                    bool asSingleton = false,
                                    bool overrideExisting = false,
                                    Func<object[], object> instanceFunc = null,
                                    object instanceObject = null)
        {
            CreateResolverCore(T,
                               C,
                               key,
                               asSingleton,
                               overrideExisting,
                               instanceFunc,
                               instanceObject);
        }

        private static bool TypeCheck(Type T,
                                      Type C,
                                      out Type correctedType)
        {
            correctedType = T;
            //valueType.FindInterfaces(((type,
            //                           criteria) =>
            //                          {
            //                              return (type.ToString() == criteria?.ToString());
            //                          }),
            //                         valueType.Name)
            //         .Any();
            if (C != correctedType)
                correctedType = C;
            else
                return false;

            return true;
        }

        private static bool TypeCheck<T>(Type C,
                                         out Type correctedType)
        {
            return TypeCheck(typeof(T), C, out correctedType);
        }

        private static bool TypeCheck<T, C>(out Type correctedType) { return TypeCheck(typeof(T), typeof(C), out correctedType); }

        bool IDIChildContainer.IsTypeRegisteredByParent<T>() { return ((IDIChildContainer) this).IsTypeRegisteredByParent(typeof(T)); }


        bool IDIChildContainer.IsTypeRegisteredByParent(Type T)
        {
            //m_lockSlim.TryEnter();
            if (m_parentContainer == null) return false;

            foreach (var c in m_parentContainer.ChildContainers)
            {
                if (c.Id == m_id) continue;

                if (c.IsTypeRegistered(T, DIResolutionStrategy.NoParent)) return true;
            }

            return m_parentContainer.IsTypeRegistered(T, DIResolutionStrategy.NoChildren);
        }


        bool IDIParentContainer.IsTypeRegisteredByChild<T>() { return ((IDIParentContainer) this).IsTypeRegisteredByChild(typeof(T)); }


        bool IDIParentContainer.IsTypeRegisteredByChild(Type T)
        {
            return m_childContainers.Select(c => c.Value)
                                    .Any(c => c.IsTypeRegistered(T, DIResolutionStrategy.NoParent));
        }

        private bool CheckResolvers(Type T,
                                    Type cT,
                                    out ResolverDictionary resolverDictionary)
        {
            resolverDictionary = null;
            m_typeDictionaryLockSlim.TryEnter();
            try
            {
                if (m_typeDictionary.TryGetValue(T, out resolverDictionary)) 
                    return true;

                if (m_typeDictionary.TryGetValue(cT, out resolverDictionary)) 
                    return true;
            }
            finally
            {
                m_typeDictionaryLockSlim.TryExit();
            }

            return false;
        }

        private void CreateResolverCore(Type T,
                                        Type cT,
                                        string key,
                                        bool asSingleton,
                                        bool overrideExisting,
                                        Func<object[], object> instanceFunc = null,
                                        object instanceObject = null)
        {
            m_typeDictionaryLockSlim.TryEnter(SynchronizationAccess.Write);
            try
            {
                if (instanceFunc is null &&
                    (cT.IsInterface || cT.IsAbstract))
                    throw new IoCResolutionException();
                //Throw.If<IoCRegistrationException>(cT.IsInterface || cT.IsAbstract);

                key ??= ResolveKey(cT);

                RegisterInterfaces(T, cT);

                //key = GetKeyValueOrDefault(key) ?? key;
                //ResolverDictionary resolverDictionary = null;
                if (!CheckResolvers(T, cT, out var resolverDictionary)) resolverDictionary ??= new ResolverDictionary();

                if (asSingleton && resolverDictionary.TryGetValue(key, out var r)) return;

                var resolver = instanceFunc is not null
                                   ? new Resolver(this, cT, asSingleton, instanceFunc)
                                   : instanceObject is not null
                                       ? new Resolver(this, cT, asSingleton, instanceObject)
                                       : new Resolver(this, cT, asSingleton);
                resolver.Initialize();
                resolverDictionary[key] = resolver;
                m_typeDictionary[cT]    = resolverDictionary;
                m_keyDictionary[key]    = T;
            }
            finally
            {
                m_typeDictionaryLockSlim.TryExit(SynchronizationAccess.Write);
            }
        }

        private void RegisterInterfaces(Type T,
                                        Type cT)
        {
            if (T.IsInterface)
            {
                if (!m_interfaceDictionary.TryAdd(T, new ConcurrentHashSet<Type>(cT)))
                {
                    m_interfaceDictionary[T]
                       .Add(cT);
                }
            }

            if (T != cT)
            {
                foreach (var i in T.GetInterfaces())
                {
                    if (!m_interfaceDictionary.TryAdd(i, new ConcurrentHashSet<Type>(cT)))
                    {
                        m_interfaceDictionary[i]
                           .Add(cT);
                    }
                }
            }

            if (cT.IsInterface) return;

            foreach (var i in cT.GetInterfaces())
            {
                if (!m_interfaceDictionary.TryAdd(i, new ConcurrentHashSet<Type>(cT)))
                {
                    m_interfaceDictionary[i]
                       .Add(cT);
                }
            }
        }

        private void CreateResolver<T, C>(string key = null,
                                          bool asSingleton = false,
                                          bool overrideExisting = false,
                                          Func<object[], C> instanceFunc = null,
                                          C instanceObject = null)
            where C : class, T
        {
            CreateResolverCore(typeof(T),
                               typeof(C),
                               key,
                               asSingleton,
                               overrideExisting,
                               instanceFunc,
                               instanceObject);
        }

        private void CreateResolver<T>(Type C,
                                       string key = null,
                                       bool asSingleton = false,
                                       bool overrideExisting = false,
                                       Func<object[], T> instanceFunc = null,
                                       T instanceObject = default)
        {
            CreateResolverCore(typeof(T),
                               C,
                               key,
                               asSingleton,
                               overrideExisting,
                               p => instanceFunc(p),
                               instanceObject);
        }

        //private void CreateResolver<T>(string key,
        //                               bool asSingleton,
        //                               bool overrideExisting,
        //                               Func<object[], T> instanceFunc = null,
        //                               T instanceObject = null)
        //    //where T : class
        //{
        //    CreateResolverCore(typeof(T),
        //                       typeof(T),
        //                       key,
        //                       asSingleton,
        //                       overrideExisting,
        //                       instanceFunc);
        //}

        private void CreateResolver<T>(string key = null,
                                       bool asSingleton = false,
                                       bool overrideExisting = false,
                                       Func<object[], T> instanceFunc = null,
                                       T instanceObject = default)
            //where T : class
        {
            CreateResolver(typeof(T),
                           typeof(T),
                           key,
                           asSingleton,
                           overrideExisting);
        }
        #endregion

        //private void CreateResolver(Type T,
        //                            Func<object> getInstanceFunc,
        //                            string key,
        //                            bool asSingleton,
        //                            bool overrideExisting)
        //{
        //    Func<object[], object> iFunc = getInstanceFunc is null ? null : p => getInstanceFunc();
        //    CreateResolverCore(T,
        //                       T,
        //                       key,
        //                       asSingleton,
        //                       overrideExisting,
        //                       iFunc);
        //}
    }
}