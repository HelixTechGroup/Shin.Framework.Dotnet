﻿#region Usings
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Shield.Framework.IoC.Native.DependencyInjection.Exceptions;
using Shin.Framework;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.IoC.DependencyInjection;
#endregion

namespace Shield.Framework.IoC.Native.DependencyInjection
{
    public sealed class IoCContainer : Disposable, IContainer
    {
        #region Members
        private readonly ReaderWriterLockSlim m_constructorDictionaryLockSlim;
        private readonly string m_defaultKey;
        private readonly ReaderWriterLockSlim m_lockSlim;
        private ConcurrentDictionary<Type, ConstructorInvokeInfo> m_constructorDictionary;
        private ConcurrentDictionary<Type, ConcurrentList<string>> m_cycleDictionary;
        private ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>> m_injectablePropertyDictionary;

        private ConcurrentDictionary<string, Type> m_keyDictionary;
        private ConcurrentDictionary<string, Action<object, object>> m_propertyActionDictionary;
        private ConcurrentDictionary<Type, PropertyInfo[]> m_propertyDictionary;
        private ConcurrentDictionary<Type, ResolverDictionary> m_typeDictionary;
        private ConcurrentDictionary<string, IContainer> m_childContainers;
        private IContainer m_parentContainer;
        #endregion

        public IoCContainer(IContainer parent)
        {
            m_typeDictionary = new ConcurrentDictionary<Type, ResolverDictionary>();
            m_keyDictionary = new ConcurrentDictionary<string, Type>();
            m_defaultKey = Guid.NewGuid().ToString();
            m_lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            m_constructorDictionaryLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            m_cycleDictionary = new ConcurrentDictionary<Type, ConcurrentList<string>>();
            m_constructorDictionary = new ConcurrentDictionary<Type, ConstructorInvokeInfo>();
            m_injectablePropertyDictionary = new ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>>();
            m_propertyActionDictionary = new ConcurrentDictionary<string, Action<object, object>>();
            m_propertyDictionary = new ConcurrentDictionary<Type, PropertyInfo[]>();
            m_childContainers = new ConcurrentDictionary<string, IContainer>();
            m_parentContainer = parent;

            Register<IContainer>(this);
        }

        public IoCContainer() : this(null) { }

        #region Methods
        public IContainer CreateChildContainer()
        {
            var container =  new IoCContainer(this);
            m_childContainers[Guid.NewGuid().ToString()] = container;
            return container;
        }

        public void Load(params IBindings[] bindings)
        {
            throw new NotImplementedException();
        }

        public void Unload(params IBindings[] bindings)
        {
            throw new NotImplementedException();
        }

        public void Register<T>(T value, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver<T>(() => value, key, asSingleton, overrideExisting);
        }

        public void Register(object value, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(value.GetType(), () => value, key, asSingleton, overrideExisting);
        }

        public void Register<T>(bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver<T>(() => Instantiate(typeof(T)), key, asSingleton, overrideExisting);
        }

        public void Register(Type T, object value, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(T, () => value, key, asSingleton, overrideExisting);
        }

        public void Register(Type T, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(T, () => Instantiate(T), key, asSingleton, overrideExisting);
        }

        /// <inheritdoc />
        public void Register<T>(Type C, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(typeof(T), () => Instantiate(C), key, asSingleton, overrideExisting);
        }

        public void Register<T, C>(C value, bool asSingleton = true, string key = null, bool overrideExisting = false) where C : class, T
        {
            CreateResolver(typeof(T), () => value, key, asSingleton, overrideExisting);
        }

        public void Register<T, C>(bool asSingleton = true, string key = null, bool overrideExisting = false) where C : class, T
        {
            CreateResolver<T>(() => Instantiate(typeof(C)), key, asSingleton, overrideExisting);
        }

        public void Register(Type T, Type C, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(T, () => Instantiate(C), key, asSingleton, overrideExisting);
        }

        public void Unregister<T>(string key = null)
        {
            throw new NotImplementedException();
        }

        public void Unregister(Type T, string key = null)
        {
            throw new NotImplementedException();
        }

        public void Unregister(object value)
        {
            throw new NotImplementedException();
        }

        public void UnregisterAll(Type T)
        {
            throw new NotImplementedException();
        }

        public void UnregisterAll<T>()
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>(string key = null, params object[] parameters)
        {
            return (T)ResolveAux(typeof(T), key, null, parameters);
        }

        public object Resolve(Type T, string key = null, params object[] parameters)
        {
            return ResolveAux(T, key, null, parameters);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            var fromType = typeof(T);
            var list = new ConcurrentList<T>();

            ResolverDictionary dictionary;
            bool retrieved;

            m_lockSlim.EnterReadLock();
            try
            {
                retrieved = m_typeDictionary.TryGetValue(fromType, out dictionary);
            }
            finally
            {
                m_lockSlim.ExitReadLock();
            }

            if (retrieved)
                list.AddRange(dictionary.Values.Select(resolver => (T)resolver.GetObject()));

            return list;
        }

        public IEnumerable<object> ResolveAll(Type T)
        {
            var list = new ConcurrentList<object>();

            ResolverDictionary resolverDictionary;
            bool retrievedValue;

            m_lockSlim.EnterReadLock();
            try
            {
                retrievedValue = m_typeDictionary.TryGetValue(T, out resolverDictionary);
            }
            finally
            {
                m_lockSlim.ExitReadLock();
            }

            if (retrievedValue)
            {
                if (resolverDictionary == null)
                {
                    var builtObject = BuildUp(T, null);
                    if (builtObject != null)
                        list.Add(builtObject);
                }
                else
                    list.AddRange(resolverDictionary.Values.Select(resolver => resolver.GetObject()));
            }

            return list;
        }

        public T TryResolve<T>(string key = null)
        {
            try
            {
                return Resolve<T>(key);
            }
            catch (IoCResolutionException e)
            {
                return default;
            }
        }

        public object TryResolve(Type T, string key = null)
        {
            try
            {
                return Resolve(T, key);
            }
            catch (IoCResolutionException e)
            {
                return null;
            }
        }

        public void Release()
        {
            foreach (var i in m_typeDictionary.Values)
                i.Dispose();

            m_typeDictionary = new ConcurrentDictionary<Type, ResolverDictionary>();
            m_keyDictionary = new ConcurrentDictionary<string, Type>();
            m_cycleDictionary = new ConcurrentDictionary<Type, ConcurrentList<string>>();
            m_constructorDictionary = new ConcurrentDictionary<Type, ConstructorInvokeInfo>();
            m_injectablePropertyDictionary = new ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>>();
            m_propertyActionDictionary = new ConcurrentDictionary<string, Action<object, object>>();
            m_propertyDictionary = new ConcurrentDictionary<Type, PropertyInfo[]>();
        }

        public bool IsRegistered<T>()
        {
            return IsRegistered(typeof(T));
        }

        public bool IsRegistered(Type T)
        {
            m_lockSlim.EnterReadLock();
            try
            {
                ResolverDictionary dictionary;
                if (m_typeDictionary.TryGetValue(T, out dictionary) && dictionary != null && dictionary.ContainsKey(m_defaultKey))
                    return true;
            }
            finally
            {
                m_lockSlim.ExitReadLock();
            }

            return false;
        }

        protected override void DisposeManagedResources()
        {
            Release();
        }

        private object ResolveAux(Type type, string key = null, Dictionary<string, object> resolvedObjects = null, params object[] parameters)
        {
            var alreadyResolved = false;
            object result = null;
            if (resolvedObjects != null && key != null)
                if (resolvedObjects.TryGetValue(key, out result))
                    alreadyResolved = true;

            if (!alreadyResolved)
            {
                result = ResolveCore(type, key);

                if (key != null && resolvedObjects != null) resolvedObjects[key] = result;

                if (result != null) ResolveProperties(result, resolvedObjects);
            }

            resolvedObjects?.Clear();

            if (result == null)
            {
                if (m_parentContainer == null)
                    throw new IoCResolutionException();

                result = m_parentContainer.Resolve(type, key);

            }
            return result;
        }

        private object ResolveCore(Type type, string key, params object[] parameters)
        {
            key = GetKeyValueOrDefault(key);

            if (type == null)
            {
                type = ResolveType(key);

                if (type == null) throw new IoCResolutionException("Failed to resolve type for " + key);
            }

            ResolverDictionary resolvers;
            IResolver resolver = null;

            m_lockSlim.EnterReadLock();
            try
            {
                if (m_typeDictionary.TryGetValue(type, out resolvers) && resolvers != null)
                {
                    /* If the ResolveProperties method calls through here, it may be searching for a registration per
                     * strongly-typed name that does not exist. In this case, just get the default registration. */
                    if (!resolvers.TryGetValue(key, out resolver))
                    {
                        key = GetKeyValueOrDefault(null);
                        resolver = resolvers[key];
                    }
                }
            }
            finally
            {
                m_lockSlim.ExitReadLock();
            }

            if (resolvers == null) return BuildUp(type, key, parameters);

            if (resolver != null) return resolver.GetObject();

            return BuildUp(type, key);
        }

        private Type ResolveType(string key)
        {
            var result = GetTypeFromContainer(key);
            if (result != null) return result;

            /*  Not in the container? Try the Assembly. */
#if NETFX_CORE
            return GetType().GetTypeInfo().Assembly.GetType(key);
#else
            return Assembly.GetExecutingAssembly().GetTypes().SingleOrDefault(t => t.Name == key);
#endif
        }

        private Type GetTypeFromContainer(string key)
        {
            Type result;
            if (!m_keyDictionary.TryGetValue(key, out result)) return null;

            return result;
        }

        private void ResolveProperties(object instance, Dictionary<string, object> resolvedObjects)
        {
            var type = instance.GetType();
            ConcurrentList<PropertyInfo> injectableProperties;

            m_lockSlim.EnterReadLock();
            try
            {
                var hasCachedInjectableProperties = m_injectablePropertyDictionary.TryGetValue(type, out injectableProperties);

                if (!hasCachedInjectableProperties)
                {
                    PropertyInfo[] properties;
                    var hasCachedProperties = m_propertyDictionary.TryGetValue(type, out properties);

                    if (!hasCachedProperties)
                    {
#if NETFX_CORE
                        properties = type.GetTypeInfo().DeclaredProperties.ToArray();
#else
                        properties = type.GetProperties();
#endif
                        m_propertyDictionary[type] = properties;
                    }

                    injectableProperties = new ConcurrentList<PropertyInfo>();

                    foreach (var propertyInfo in properties)
                    {
                        var dependencyAttributes = propertyInfo.GetCustomAttributes(typeof(InjectPropertyAttribute), false);
#if NETFX_CORE
                        if (!dependencyAttributes.Any())
#else
                        if (dependencyAttributes.Length < 1) /* Faster than using LINQ. */
#endif
                            continue;

                        injectableProperties.Add(propertyInfo);
                    }

                    m_injectablePropertyDictionary[type] = injectableProperties;
                }
            }
            finally
            {
                m_lockSlim.ExitReadLock();
            }

            if (injectableProperties == null || injectableProperties.Count < 1)
                return;

            if (resolvedObjects == null)
                resolvedObjects = new Dictionary<string, object>();

            var typeName = type.FullName + ".";

            foreach (var propertyInfo in injectableProperties)
            {
                var fullPropertyName = typeName + propertyInfo.Name;

                var propertyValue = ResolveAux(propertyInfo.PropertyType, fullPropertyName, resolvedObjects);

                Action<object, object> setter;

                if (!m_propertyActionDictionary.TryGetValue(fullPropertyName, out setter))
                {
                    setter = ReflectionCompiler.CreateSetter(propertyInfo);
                    m_propertyActionDictionary[fullPropertyName] = setter;
                }

                setter(instance, propertyValue);
            }
        }

        private object BuildUp(Type type, string key, params object[] parameters)
        {
            if (type == null)
                throw new IoCResolutionException("type cannot not be null.");

            object instance = null;

            m_lockSlim.EnterReadLock();
            try
            {
                ResolverDictionary resolverDictionary;
                if (m_typeDictionary.TryGetValue(type, out resolverDictionary))
                {
                    IResolver resolver;
                    if (resolverDictionary.TryGetValue(key, out resolver))
                        instance = resolver.GetObject();
                }
            }
            finally
            {
                m_lockSlim.ExitReadLock();
            }

            return instance ?? Instantiate(type, parameters);
        }

        private object Instantiate(ConstructorInvokeInfo info, params object[] parameters)
        {
            var constructorParameters = info.ParameterInfos;
            var length = constructorParameters.Length;
            var test = parameters.Length == length;
            var parametersList = test ? parameters : new object[length];
            if (!test)
            {
                for (var i = 0; i < length; i++)
                {
                    var parameterInfo = constructorParameters[i];
                    var parameter = Resolve(parameterInfo.ParameterType);
                    if (parameter == null && !parameterInfo.IsOptional)
                    {
                        throw new IoCResolutionException(
                                                         "Failed to instantiate parameter " + parameterInfo.Name);
                    }

                    parametersList[i] = parameter;
                }
            }

            var constructorFunc = info.ConstructorFunc;
            try
            {
                return constructorFunc(parametersList);
            }
            catch (Exception ex)
            {
                throw new IoCResolutionException("Failed to resolve " + info.Constructor.DeclaringType, ex);
            }
        }

        private object Instantiate(Type type, params object[] parameters)
        {
            ConstructorInvokeInfo invokeInfo;
            m_constructorDictionaryLockSlim.EnterReadLock();

            try
            {
                if (m_constructorDictionary.TryGetValue(type, out invokeInfo))
                    return Instantiate(invokeInfo);
            }
            finally
            {
                m_constructorDictionaryLockSlim.ExitReadLock();
            }

#if NETFX_CORE
            var constructors = type.GetTypeInfo().DeclaredConstructors.Where(x => !x.IsStatic && x.IsPublic).ToArray();
#else
            var constructors = type.GetConstructors();
#endif

            ConstructorInfo constructor = null;

            if (constructors.Length == 1)
                constructor = constructors[0];
            else if (constructors.Length > 1)
            {
                ConstructorInfo bestMatch = null;
                var biggestLength = -1;

                foreach (var constructorInfo in constructors)
                {
                    var dependencyAttributes = constructorInfo.GetCustomAttributes(typeof(InjectConstructorAttribute), false);
#if NETFX_CORE
                    var attributeCount = dependencyAttributes.Count();
#else
                    var attributeCount = dependencyAttributes.Length;
#endif
                    var hasAttribute = attributeCount > 0;

                    if (hasAttribute)
                    {
                        constructor = constructorInfo;
                        break;
                    }

                    if (constructors.Length >= 1)
                    {
                        var length = constructorInfo.GetParameters().Length;

                        if (length > biggestLength)
                        {
                            biggestLength = length;
                            bestMatch = constructorInfo;
                        }
                    }
                }

                if (constructor == null) constructor = bestMatch;
            }
            else
            {
#if !NETFX_CORE
                ConstructorInfo bestMatch = null;
                var biggestLength = -1;

                constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

                if (constructors.Length > 0)
                {
                    foreach (var constructorInfo in constructors)
                    {
                        if (constructorInfo.GetCustomAttributes(typeof(InjectConstructorAttribute), false).Length > 0)
                        {
                            constructor = constructorInfo;
                            break;
                        }

                        var length = constructorInfo.GetParameters().Length;

                        if (length <= biggestLength)
                            continue;

                        biggestLength = length;
                        bestMatch = constructorInfo;
                    }

                    if (constructor == null)
                        constructor = bestMatch;
                }
#endif
            }

            if (constructor == null)
            {
                throw new IoCResolutionException(
                                                 "Could not locate a constructor for " + type.FullName);
            }

            invokeInfo = new ConstructorInvokeInfo(constructor);
            m_constructorDictionaryLockSlim.EnterWriteLock();
            try
            {
                m_constructorDictionary[type] = invokeInfo;
            }
            finally
            {
                m_constructorDictionaryLockSlim.ExitWriteLock();
            }

            return Instantiate(invokeInfo, parameters);
        }

        private string GetKeyValueOrDefault(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                key = m_defaultKey;

            return key;
        }

        private void CreateResolver<T>(Func<object> getInstanceFunc, string key, bool asSingleton, bool overrideExisting)
        {
            CreateResolver(typeof(T), getInstanceFunc, key, asSingleton, overrideExisting);
        }

        private void CreateResolver(Type T, Func<object> getInstanceFunc, string key, bool asSingleton, bool overrideExisting)
        {
            m_lockSlim.EnterWriteLock();
            try
            {
                key = GetKeyValueOrDefault(key);
                ResolverDictionary resolverDictionary;
                if (overrideExisting)
                    resolverDictionary = new ResolverDictionary();
                else if (!m_typeDictionary.TryGetValue(T, out resolverDictionary))
                    resolverDictionary = new ResolverDictionary();

                var resolver = new TypeResolver
                               {
                                   CreateInstanceFunc = getInstanceFunc,
                                   Singleton = asSingleton
                               };
                resolverDictionary[key] = resolver;
                m_typeDictionary[T] = resolverDictionary;
                m_keyDictionary[key] = T;
            }
            finally
            {
                m_lockSlim.ExitWriteLock();
            }
        }
        #endregion
    }
}