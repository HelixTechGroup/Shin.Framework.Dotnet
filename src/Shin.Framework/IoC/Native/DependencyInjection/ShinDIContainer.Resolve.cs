#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public T Resolve<T>(string key = null, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters)
        {
            //return (T)ResolveAux(typeof(T), key, strategy, null, parameters);
            return (T)Resolve(typeof(T), key, strategy, parameters);
        }

        public object Resolve(Type T, string key = null, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters)
        {
            m_lockSlim.TryEnter();
            try
            {
                return ResolveAux(T, key, strategy, null, parameters) ?? BuildUp(T, key, parameters);
            }
            finally
            {
                m_lockSlim.TryExit();
            }
        }

        public IEnumerable<T> ResolveAll<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters)
        {
            //var fromType = typeof(T);
            //var list = new ConcurrentList<T>();

            //ResolverDictionary dictionary;
            //var retrieved = false;

            //m_lockSlim.TryEnter();
            //try
            //{
            //    if (m_typeDictionary.TryGetValue(fromType, out dictionary))
            //        retrieved = true;
            //    if (fromType.IsInterface)
            //    {
            //        foreach (var k in m_typeDictionary)
            //        {
            //            if (!k.Key.ContainsInterface(fromType))
            //                continue;

            //            //foreach (var r in k.Value.Values)
            //            //    if (r.HasInstance)
            //            //        list.Add((T)r.GetObject());
            //            list.AddRange(k.Value.Values
            //                            //.Where(resolver => resolver.HasInstance)
            //                           .Select(resolver => (T)resolver.GetObject()));
            //        }
            //    }
            //}
            //finally
            //{
            //    m_lockSlim.TryExit();
            //}

            //if (retrieved)
            //{
            //    list.AddRange(dictionary.Values
            //                             //.Where(resolver => resolver.HasInstance)
            //                            .Select(resolver => (T)resolver.GetObject()));
            //}

            //return list;

            var list = ResolveAll(typeof(T), strategy, parameters);
            var finalList = new ConcurrentList<T>();
            foreach (var o in list)
                finalList.Add((T)o);

            return finalList;
        }

        public IEnumerable<object> ResolveAll(Type T, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters)
        {
            //var list = new ConcurrentList<object>();
            //ResolverDictionary resolverDictionary;
            //bool retrievedValue;

            m_lockSlim.TryEnter();
            try
            {
                return ResolveAllAux(T, strategy, null, parameters);
            }
            finally
            {
                m_lockSlim.TryExit();
            }

            //if (retrievedValue)
            //{
            //    if (resolverDictionary == null)
            //    {
            //        var builtObject = BuildUp(T, null);
            //        if (builtObject != null)
            //            list.Add(builtObject);
            //    }
            //    else
            //    {
            //        list.AddRange(resolverDictionary.Values
            //                                         //.Where(resolver => resolver.HasInstance)
            //                                        .Select(resolver => resolver.GetObject()));
            //    }
            //}


            //return list;
        }

        public bool TryResolve<T>(out T result,
                                  string key = null,
                                  DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                  params object[] parameters)
        {
            result = default(T);

            try
            {
                result = Resolve<T>(key, strategy, parameters);
                return result is not null;
            }
            catch (IoCResolutionException e)
            {
                return false;
            }
        }

        public bool TryResolveAll<T>(out IEnumerable<T> result,
                                     DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                     params object[] parameters)
        {
            result = null;

            try
            {
                result = ResolveAll<T>(strategy, parameters);
                return result is not null && result.Any();
            }
            catch (IoCResolutionException e)
            {
                return false;
            }
        }
        
        public bool TryResolveAll(Type T,
                                  out IEnumerable result,
                                  DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                  params object[] parameters)
        {
            result = null;

            try
            {
                result = ResolveAll(T, strategy, parameters);
                return result is not null;
            }
            catch (IoCResolutionException e)
            {
                return false;
            }
        }
        
        public bool TryResolve(Type T,
                               out object result,
                               string key = null,
                               DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                               params object[] parameters)
        {
            result = null;

            try
            {
                result = Resolve(T, key, strategy, parameters);
                return result is not null;
            }
            catch (IoCResolutionException e)
            {
                return false;
            }
        }

        public void Release()
        {
            foreach (var i in m_typeDictionary.Values)
                i.Dispose();

            m_typeDictionary.Clear(); // = new ConcurrentDictionary<Type, ResolverDictionary>();
            m_keyDictionary.Clear(); //  = new ConcurrentDictionary<string, Type>();
            m_cycleDictionary.Clear(); //  = new ConcurrentDictionary<Type, ConcurrentList<string>>();
            //m_constructorDictionary.Clear(); //  = new ConcurrentDictionary<Type, ConstructorInvokeInfo[]>();
            m_injectablePropertyDictionary.Clear(); //  = new ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>>();
            m_propertyActionDictionary.Clear(); //  = new ConcurrentDictionary<string, Action<object, object>>();
            m_propertyDictionary.Clear(); //  = new ConcurrentDictionary<Type, PropertyInfo[]>();
        }

        /// <inheritdoc />
        public string ResolveKey<T>()
        {
            return ResolveKey(typeof(T));
        }

        public string ResolveKey(Type type)
        {
            Throw.If(type is null).ArgumentNullException(nameof(type));

            try
            {
                return type?.GUID.ToString();
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }

            //            string key = null;
            //            if (!GetKeyFromContainer(type, out key))
            //            {
            //#if NETFX_CORE
            //                type = GetType().GetTypeInfo().Assembly.GetType(key);
            //#else
            //                key = Assembly.GetExecutingAssembly()
            //                               .GetTypes()
            //                               .SingleOrDefault(t => (t == type) type.GUID);
            //#endif
            //            }

            //            return key;
        }

        public Type ResolveType(string key)
        {
            Throw.If(string.IsNullOrWhiteSpace(key)).ArgumentNullException(nameof(key));

            if (GetTypeFromContainer(key, out var type))
                return type;


#if NETFX_CORE
                type = GetType().GetTypeInfo().Assembly.GetType(key);
#else
            var types = Assembly.GetExecutingAssembly()
                                .GetTypes();
            foreach (var t in types)
            {
                if (t.GUID.ToString() == key)
                    return t;
            }
            //.SingleOrDefault(t => t.GUID.ToString() == key);
#endif


            //if (type == null)
            //    throw new IoCResolutionException("Failed to resolve type for " + key);

            return null;
        }

        private IEnumerable<object> ResolveAllSelf(Type type, string key = null, params object[] parameters)
        {
            var list = new HashSet<object>();
            if (!InterfaceCheck(type, out var cType))
            {
                m_typeDictionaryLockSlim.TryEnter();
                try
                {
                    if (m_typeDictionary.TryGetValue(type, out var resolvers))
                        list.AddRange(resolvers.Values.Select(r => r?.GetObject(parameters)));
                }
                finally
                {
                    m_typeDictionaryLockSlim.TryExit();
                }
            }

            foreach (var t in cType)
            {
                m_typeDictionaryLockSlim.TryEnter();
                try
                {
                    if (!m_typeDictionary.TryGetValue(t, out var resolvers)) continue;

                    list.AddRange(resolvers.Values.Select(r => r?.GetObject(parameters)));
                }
                finally
                {
                    m_typeDictionaryLockSlim.TryExit();
                }
            }
            
            //m_interfaceDictionaryLockSlim.TryEnter();
            //if (m_interfaceDictionary.ContainsKey(type))
            //{
                //var i = m_interfaceDictionary[type];
                //foreach (var k in i)
                //{
                    //if (!k.Key.ContainsInterface(T))
                    //    continue;

                    //foreach (var r in k.Value.Values)
                    //    if (r.HasInstance)
                    //        list.Add(r.GetObject());
            //        var r = m_typeDictionary[k];
            //        list.AddRange(r.Values
            //                        //.Where(resolver => resolver.HasInstance)
            //                       .Select(resolver => resolver.GetObject()));
            //    }
            //}

            return list;
        }

        private object ResolveSelf(Type type, string key = null, params object[] parameters)
        {
            InterfaceCheck(type, out var cType);
            foreach (var t in cType)
            {
                if (!CheckResolvers(type, t, out var resolvers))
                    continue;

                if (!resolvers.ContainsKey(key))
                    key = ResolveKey(t);
                var result = resolvers[key]?.GetObject(parameters);
                if (result is not null) 
                    return result;
            }
            //m_typeDictionaryLockSlim.TryEnter();
                
            //try
            //{
                //if (m_typeDictionary.TryGetValue(type, out var resolvers))
                //{
                //    foreach (var resolver in resolvers.Values)
                //    {
                //        var result = resolver.GetObject(parameters);
                //        if (result is not null)
                //            return result;
                //    }
                //}
            //}
            //finally
            //{
            //    m_typeDictionaryLockSlim.TryExit();
            //}

            return null; //BuildUp(type, key, parameters);
        }

        private IEnumerable<object> ResolveAllChildren(Type type,
                                                       string key = null,
                                                       DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                                       params object[] parameters)
        {
            var list = new ConcurrentList<object>();
            foreach (var c in m_childContainers.Values)
            {
                if (c.IsTypeRegistered(type, DIResolutionStrategy.NoParent))
                    list.AddRange(c.ResolveAll(type, DIResolutionStrategy.NoParent, parameters));
            }

            return list;
        }

        private object ResolveChildren(Type type,
                                       string key = null,
                                       DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                       params object[] parameters)
        {
            foreach (var c in m_childContainers.Values)
            {
                if (c.IsTypeRegistered(type, DIResolutionStrategy.NoParent))
                    return c.Resolve(type, key, DIResolutionStrategy.NoParent, parameters);
            }

            return null;
            //foreach (var c in m_childContainers
            //                 .Select(ct => ct.Value)
            //                 .Where(cc => cc.IsKeyRegistered(key))
            //                 .Select(o => o.Resolve(type, key)))
            //    return c;
        }

        private IEnumerable<object> ResolveAllParent(Type type,
                                                     string key = null,
                                                     DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                                     params object[] parameters)
        {
            var list = new ConcurrentList<object>();
            if (m_parentContainer != null)
            {
                //if (!m_parentContainer.IsTypeRegistered(type, DIResolutionStrategy.IncludeSelf |
                //                                              DIResolutionStrategy.ParentLast))
                //    return null;

                foreach (var c in m_parentContainer.ChildContainers)
                {
                    if (c.Id == m_id)
                        continue;

                    if (c.IsTypeRegistered(type, DIResolutionStrategy.NoParent))
                        list.AddRange(c.ResolveAll(type, DIResolutionStrategy.NoParent, parameters));

                    //if (c.IsTypeRegistered(type))
                    //    return c.Resolve(type, key, DIResolutionStrategy.IncludeSelf | 
                    //                                DIResolutionStrategy.ChildrenFirst, parameters);
                }

                if (m_parentContainer.IsTypeRegistered(type, DIResolutionStrategy.NoChildren))
                    list.AddRange(m_parentContainer.ResolveAll(type, DIResolutionStrategy.NoChildren, parameters));
            }

            return list;
        }

        private object ResolveParent(Type type,
                                     string key = null,
                                     DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                     params object[] parameters)
        {
            if (m_parentContainer != null)
            {
                //if (!m_parentContainer.IsTypeRegistered(type, DIResolutionStrategy.IncludeSelf |
                //                                              DIResolutionStrategy.ParentLast))
                //    return null;

                foreach (var c in m_parentContainer.ChildContainers)
                {
                    if (c.Id == m_id)
                        continue;

                    if (c.IsTypeRegistered(type, DIResolutionStrategy.NoParent))
                        return c.Resolve(type, key, DIResolutionStrategy.NoParent, parameters);

                    //if (c.IsTypeRegistered(type))
                    //    return c.Resolve(type, key, DIResolutionStrategy.IncludeSelf | 
                    //                                DIResolutionStrategy.ChildrenFirst, parameters);
                }

                if (m_parentContainer.IsTypeRegistered(type, DIResolutionStrategy.NoChildren))
                    return m_parentContainer.Resolve(type, key, DIResolutionStrategy.NoChildren, parameters);
            }

            return null;
        }

        private IEnumerable<object> ResolveAllAux(Type type,
                                                  DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                                  IDictionary<string, object> resolvedObjects = null,
                                                  params object[] parameters)
        {
            var tmp = new ConcurrentList<object>();
            tmp.AddRange(ResolveAllCore(type, strategy, parameters));
            //if (tmp.Count == 0)
            //    BuildUp(type, null, parameters);

            var list = new ConcurrentList<object>();
            foreach (var o in tmp)
            {
                var t = o;
                ResolveProperties(ref t, strategy, resolvedObjects);
                list.Add(t);
            }

            return list;
        }

        private object ResolveAux(Type type,
                                  string key = null,
                                  DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                  IDictionary<string, object> resolvedObjects = null,
                                  params object[] parameters)
        {
            var alreadyResolved = false;
            object result = null;
            if (resolvedObjects != null && key != null)
            {
                if (resolvedObjects.TryGetValue(key, out result))
                    alreadyResolved = true;
            }

            if (!alreadyResolved)
            {
                //if (InterfaceCheck(type, out var concrete))
                //    type = concrete;

                result = ResolveCore(type, key, strategy, parameters);
                result ??= BuildUp(type, key, parameters);
            }

            if (result != null)
                ResolveProperties(ref result, strategy, resolvedObjects);

            resolvedObjects?.Clear();

            return result;
        }

        private IEnumerable<object> ResolveAllCore(Type type,
                                                   DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                                   params object[] parameters)
        {
            //m_lockSlim.TryEnter(SynchronizationAccess.Write);

            try
            {
                var key = ResolveKey(type);

                var list = new HashSet<object>();
                var selfOnly = strategy.HasFlag(DIResolutionStrategy.SelfOnly);

                if (!selfOnly)
                {
                    if (strategy
                           .HasFlag(DIResolutionStrategy.ChildrenOnly) ||
                        strategy.HasFlag(DIResolutionStrategy.ChildrenFirst))
                    {
                        list.AddRange(ResolveAllChildren(type,
                                                         key,
                                                         DIResolutionStrategy.IncludeSelf |
                                                         DIResolutionStrategy.ChildrenLast,
                                                         parameters));
                    }

                    if ((strategy.HasFlag(DIResolutionStrategy.ParentFirst) ||
                         strategy.HasFlag(DIResolutionStrategy.ParentOnly)))
                        list.AddRange(ResolveAllParent(type, key, strategy, parameters));
                }

                if ((strategy.HasFlag(DIResolutionStrategy.IncludeSelf) ||
                     selfOnly))
                    list.AddRange(ResolveAllSelf(type, key, parameters));

                if (!selfOnly)
                {
                    if (strategy.HasFlag(DIResolutionStrategy.ChildrenLast))
                    {
                        list.AddRange(ResolveAllChildren(type,
                                                         key,
                                                         DIResolutionStrategy.IncludeSelf |
                                                         DIResolutionStrategy.ChildrenLast,
                                                         parameters));
                    }

                    if (strategy.HasFlag(DIResolutionStrategy.ParentLast))
                        list.AddRange(ResolveAllParent(type, key, strategy, parameters));
                }

                return list;
            }
            finally
            {
                //m_lockSlim.TryExit(SynchronizationAccess.Write);
            }
        }

        private object ResolveCore(Type type,
                                   string key = null,
                                   DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                   params object[] parameters)
        {
            //m_lockSlim.TryEnter(SynchronizationAccess.Write);

            try
            {
                key ??= ResolveKey(type);

                object result = null;
                var selfOnly = strategy.HasFlag(DIResolutionStrategy.SelfOnly);

                if (!selfOnly)
                {
                    if (strategy
                           .HasFlag(DIResolutionStrategy.ChildrenOnly) ||
                        strategy.HasFlag(DIResolutionStrategy.ChildrenFirst))
                    {
                        result = ResolveChildren(type,
                                                 key,
                                                 DIResolutionStrategy.IncludeSelf |
                                                 DIResolutionStrategy.ChildrenLast,
                                                 parameters);
                    }

                    if (result is null &&
                        (strategy.HasFlag(DIResolutionStrategy.ParentFirst) ||
                         strategy.HasFlag(DIResolutionStrategy.ParentOnly)))
                        result = ResolveParent(type, key, strategy, parameters);
                }

                if (result is null &&
                    (strategy.HasFlag(DIResolutionStrategy.IncludeSelf) ||
                     selfOnly))
                    result = ResolveSelf(type, key, parameters);

                if (result is null && !selfOnly)
                {
                    if (strategy.HasFlag(DIResolutionStrategy.ChildrenLast))
                    {
                        result = ResolveChildren(type,
                                                 key,
                                                 DIResolutionStrategy.IncludeSelf |
                                                 DIResolutionStrategy.ChildrenLast,
                                                 parameters);
                    }

                    if (result is null && strategy.HasFlag(DIResolutionStrategy.ParentLast))
                        result = ResolveParent(type, key, strategy, parameters);
                }

                return result;
            }
            finally
            {
                //m_lockSlim.TryExit(SynchronizationAccess.Write);
            }
        }

        internal ShinDIContainer ResolveContainer(string key)
        {
            return this;
        }

        private void ResolveProperties(ref object instance,
                                       DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                       IDictionary<string, object> resolvedObjects = null)
        {
            var type = instance.GetType();
            ConcurrentList<PropertyInfo> injectableProperties;

            //m_lockSlim.TryEnter();
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
                //m_lockSlim.TryExit();
            }

            if (injectableProperties == null || injectableProperties.Count < 1)
                return;

            if (resolvedObjects == null)
                resolvedObjects = new Dictionary<string, object>();

            var typeName = type.FullName + ".";

            foreach (var propertyInfo in injectableProperties)
            {
                var fullPropertyName = typeName + propertyInfo.Name;

                var propertyValue = ResolveAux(propertyInfo.PropertyType, fullPropertyName, strategy, resolvedObjects);
                resolvedObjects.Add(propertyInfo.PropertyType.GUID.ToString(), propertyValue);

                Action<object, object> setter;

                if (!m_propertyActionDictionary.TryGetValue(fullPropertyName, out setter))
                {
                    setter = ReflectionCompiler.CreateSetter(propertyInfo);
                    m_propertyActionDictionary[fullPropertyName] = setter;
                }

                setter(instance, propertyValue);
            }
        }
        #endregion
    }
}