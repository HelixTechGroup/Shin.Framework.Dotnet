#region Usings
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.Extensions;
using Shin.Framework.IoC.DependencyInjection;
using Shin.Framework.IoC.Native.DependencyInjection.Exceptions;
using Shin.Framework.Threading;

//using System.Reflection.Metadata;
#endregion

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    public sealed class ShinDIContainer : Disposable, IDIContainer, IDIChildContainer, IDIParentContainer, IDIRootContainer
    {
        #region Members
        private readonly ConcurrentDictionary<string, IDIChildContainer> m_childContainers;
        private readonly ConcurrentDictionary<Type, ConstructorInvokeInfo[]> m_constructorDictionary;
        private readonly ReaderWriterLockSlim m_constructorDictionaryLockSlim;
        private readonly ConcurrentDictionary<Type, ConcurrentList<string>> m_cycleDictionary;
        private readonly string m_defaultKey;

        /// <inheritdoc />
        private readonly Guid m_id;

        private readonly ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>> m_injectablePropertyDictionary;
        private readonly ConcurrentDictionary<Type, ConcurrentList<Type>> m_interfaceDictionary;

        private readonly ConcurrentDictionary<string, Type> m_keyDictionary;
        private readonly ReaderWriterLockSlim m_lockSlim;
        private readonly IDIParentContainer m_parentContainer;
        private readonly ConcurrentDictionary<string, Action<object, object>> m_propertyActionDictionary;
        private readonly ConcurrentDictionary<Type, PropertyInfo[]> m_propertyDictionary;

        private readonly IDIRootContainer m_root;
        private readonly ConcurrentDictionary<Type, ResolverDictionary> m_typeDictionary;
        #endregion

        #region Properties
        /// <inheritdoc />
        public Guid Id
        {
            get { return m_id; }
        }

        /// <inheritdoc />
        public IDIRootContainer Root
        {
            get { return m_root; }
        }

        /// <inheritdoc />
        IDIChildContainer[] IDIParentContainer.ChildContainers
        {
            get { return m_childContainers.Values.ToArray(); }
        }

        /// <inheritdoc />
        IDIParentContainer IDIChildContainer.ParentContainer
        {
            get { return m_parentContainer; }
        }

        IEnumerable<Type> IDIContainer.RegisteredTypes
        {
            get { return m_typeDictionary.Keys; }
        }
        #endregion

        public ShinDIContainer(IDIContainer parent) : this((IDIParentContainer)parent) { }

        public ShinDIContainer()
        {
            m_id = Guid.NewGuid();
            m_typeDictionary = new ConcurrentDictionary<Type, ResolverDictionary>();
            m_keyDictionary = new ConcurrentDictionary<string, Type>();
            m_defaultKey = Guid.NewGuid().ToString();
            m_lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            m_constructorDictionaryLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            m_cycleDictionary = new ConcurrentDictionary<Type, ConcurrentList<string>>();
            m_constructorDictionary = new ConcurrentDictionary<Type, ConstructorInvokeInfo[]>();
            m_injectablePropertyDictionary = new ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>>();
            m_propertyActionDictionary = new ConcurrentDictionary<string, Action<object, object>>();
            m_propertyDictionary = new ConcurrentDictionary<Type, PropertyInfo[]>();
            m_childContainers = new ConcurrentDictionary<string, IDIChildContainer>();
            m_interfaceDictionary = new ConcurrentDictionary<Type, ConcurrentList<Type>>();
            m_root ??= this;
            //m_parentContainer ??= this;

            Register<IDIContainer>(this);
            Register<IDIRootContainer>(this);
            CreateResolver<IDIChildContainer>(((IDIParentContainer)this).CreateChildContainer, null, false, false);
        }

        internal ShinDIContainer(IDIParentContainer parent) : this()
        {
            Throw.IfNull(parent).ArgumentNullException(nameof(parent));

            m_parentContainer = parent;
            m_root = m_parentContainer?.Root;
            Register(m_parentContainer);
        }

        #region Methods
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
            CreateResolver<T>(p => Instantiate(typeof(T), typeof(T), p), key, asSingleton, overrideExisting);
        }

        public void Register(Type T, object value, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(T, () => value, key, asSingleton, overrideExisting);
        }

        public void Register(Type T, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(T, T, p => Instantiate(T, T, p), key, asSingleton, overrideExisting);
        }

        /// <inheritdoc />
        public void Register<T>(Type C, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(typeof(T), C, p => Instantiate(typeof(T), C, p), key, asSingleton, overrideExisting);
        }

        public void Register<T, C>(C value, bool asSingleton = true, string key = null, bool overrideExisting = false) where C : class, T
        {
            CreateResolver<T, C>(p => value, key, asSingleton, overrideExisting);
        }

        public void Register<T, C>(bool asSingleton = true, string key = null, bool overrideExisting = false) where C : class, T
        {
            CreateResolver<T, C>(p => Instantiate(typeof(T), typeof(C), p), key, asSingleton, overrideExisting);
        }

        public void Register(Type T, Type C, bool asSingleton = true, string key = null, bool overrideExisting = false)
        {
            CreateResolver(T, C, p => Instantiate(T, C, p), key, asSingleton, overrideExisting);
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

        public T Resolve<T>(string key = null, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters)
        {
            //return (T)ResolveAux(typeof(T), key, strategy, null, parameters);
            return (T)Resolve(typeof(T), key, strategy, parameters);
        }

        public object Resolve(Type T, string key = null, DIResolutionStrategy strategy = DIResolutionStrategy.Default, params object[] parameters)
        {
            return ResolveAux(T, key, strategy, null, parameters) ?? BuildUp(T, key, parameters);
        }

        public IEnumerable<T> ResolveAll<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default)
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

            var list = ResolveAll(typeof(T));
            var finalList = new ConcurrentList<T>();
            foreach (var o in list)
                finalList.Add((T)o);

            return finalList;
        }

        public IEnumerable<object> ResolveAll(Type T, DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            var list = new ConcurrentList<object>();

            ResolverDictionary resolverDictionary;
            bool retrievedValue;

            m_lockSlim.TryEnter();
            try
            {
                retrievedValue = m_typeDictionary.TryGetValue(T, out resolverDictionary);
            }
            finally
            {
                m_lockSlim.TryExit();
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
                {
                    list.AddRange(resolverDictionary.Values
                                                     //.Where(resolver => resolver.HasInstance)
                                                    .Select(resolver => resolver.GetObject()));
                }
            }

            if (T.IsInterface)
            {
                foreach (var k in m_typeDictionary)
                {
                    if (!k.Key.ContainsInterface(T))
                        continue;

                    //foreach (var r in k.Value.Values)
                    //    if (r.HasInstance)
                    //        list.Add(r.GetObject());

                    list.AddRange(k.Value.Values
                                    //.Where(resolver => resolver.HasInstance)
                                   .Select(resolver => resolver.GetObject()));
                }
            }

            return list;
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
            m_constructorDictionary.Clear(); //  = new ConcurrentDictionary<Type, ConstructorInvokeInfo[]>();
            m_injectablePropertyDictionary.Clear(); //  = new ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>>();
            m_propertyActionDictionary.Clear(); //  = new ConcurrentDictionary<string, Action<object, object>>();
            m_propertyDictionary.Clear(); //  = new ConcurrentDictionary<Type, PropertyInfo[]>();
        }

        /// <inheritdoc />
        public string ResolveKey<T>()
        {
            return ResolveKey(typeof(T));
        }

        /// <inheritdoc />
        //public Type ResolveType(string key)
        //{
        //    GetTypeFromContainer(key, out var result);
        //    return result;
        //}
        public bool IsKeyRegistered(string key, DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            return GetTypeFromContainer(key, out var type);
        }

        public bool IsTypeRegistered<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            return IsTypeRegistered(typeof(T), strategy);
        }

        public bool IsTypeRegistered(Type T, DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            //m_lockSlim.TryEnter();
            var result = false;
            var selfOnly = strategy.HasFlag(DIResolutionStrategy.SelfOnly);

            if (!selfOnly)
            {
                if (strategy
                       .HasFlag(DIResolutionStrategy.ChildrenOnly) ||
                    strategy.HasFlag(DIResolutionStrategy.ChildrenFirst))
                    result = ((IDIParentContainer)this).IsTypeRegisteredByChild(T);

                if (!result &&
                    (strategy.HasFlag(DIResolutionStrategy.ParentFirst) ||
                     strategy.HasFlag(DIResolutionStrategy.ParentOnly)))
                    result = ((IDIChildContainer)this).IsTypeRegisteredByParent(T);
            }

            if (!result &&
                (strategy.HasFlag(DIResolutionStrategy.IncludeSelf) ||
                 selfOnly))
            {
                result = m_typeDictionary.ContainsKey(T);

                if (!result && InterfaceCheck(T, out var cType) && cType is not null)
                    result = m_typeDictionary.ContainsKey(cType) /*|| m_interfaceDictionary.ContainsKey(T)*/;
            }

            if (!result && !selfOnly)
            {
                if (strategy.HasFlag(DIResolutionStrategy.ChildrenLast))
                    result = ((IDIParentContainer)this).IsTypeRegisteredByChild(T);

                if (!result && strategy.HasFlag(DIResolutionStrategy.ParentLast))
                    result = ((IDIChildContainer)this).IsTypeRegisteredByParent(T);
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            //sb.AppendLine(base.ToString());

            sb.AppendLine(@$"Container Id: {m_id}");
            sb.AppendLine($@"Root Container: {m_root?.Id}");
            sb.AppendLine($@"Parent Container: {m_parentContainer?.Id}");
            sb.AppendLine(@$"Type Dictionary Count: {m_typeDictionary.Count}");
            foreach (var t in m_typeDictionary)
            {
                sb.AppendLine(@$"Type key: {t.Key.ToString()}");
                foreach (var v in t.Value.Keys)
                    sb.AppendLine(@$"Type Value: {v}");
            }

            sb.AppendLine(@"Child Containers:");
            foreach (var child in m_childContainers)
                sb.AppendLine($@"{child.Value}");

            return sb.ToString();
        }

        public string ResolveKey(Type type)
        {
            Throw.If(type == null).ArgumentNullException(nameof(type));

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

        protected override void DisposeManagedResources()
        {
            Release();
        }

        IDIChildContainer IDIParentContainer.CreateChildContainer()
        {
            var container = new ShinDIContainer(this);
            m_childContainers[container.Id.ToString()] = container;
            return container;
        }

        /// <inheritdoc />
        bool IDIChildContainer.IsTypeRegisteredByParent<T>()
        {
            return ((IDIChildContainer)this).IsTypeRegisteredByParent(typeof(T));
        }

        /// <inheritdoc />
        bool IDIChildContainer.IsTypeRegisteredByParent(Type T)
        {
            m_lockSlim.TryEnter();
            try
            {
                if (m_parentContainer == null)
                    return false;

                foreach (var c in m_parentContainer.ChildContainers)
                {
                    if (c.Id == m_id)
                        continue;

                    if (c.IsTypeRegistered(T, DIResolutionStrategy.NoParent))
                        return true;
                }

                return m_parentContainer.IsTypeRegistered(T,
                                                          DIResolutionStrategy.NoChildren);
            }
            finally
            {
                m_lockSlim.TryExit();
            }
        }

        /// <inheritdoc />
        bool IDIParentContainer.IsTypeRegisteredByChild<T>()
        {
            return ((IDIParentContainer)this).IsTypeRegisteredByChild(typeof(T));
        }

        /// <inheritdoc />
        bool IDIParentContainer.IsTypeRegisteredByChild(Type T)
        {
            return m_childContainers
                  .Select(c => c.Value)
                  .Any(c => c.IsTypeRegistered(T, DIResolutionStrategy.NoParent));
        }

        private object ResolveSelf(Type type, string key = null, params object[] parameters)
        {
            InterfaceCheck(type, out var cType);
            if (cType is null || !m_typeDictionary.TryGetValue(cType, out var resolvers))
                return null;

            var result = resolvers.Values
                                  .Select(r => r?.GetObject(parameters))
                                  .First();
            return result;
            //return null; //BuildUp(type, key, parameters);
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
                if (type.IsPrimitive || type.IsValueType)
                    return null;

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

        private bool GetKeyFromContainer(Type type, out string key)
        {
            Throw.If(type is null).ArgumentNullException(nameof(type));

            key = null;
            if (type.IsInterface)
            {
                foreach (var k in m_keyDictionary)
                {
                    if (k.Value.ContainsInterface(type))
                        key = k.Key;
                }
            }


            if (m_keyDictionary.Values.Contains(type))
            {
                foreach (var k in m_keyDictionary)
                {
                    if (k.Value == type)
                        key = k.Key;
                }
            }

            if (!string.IsNullOrWhiteSpace(key))
                return true;

            key = type.GUID.ToString();
            return false;
        }

        private bool GetKeyFromChildContainer(Type type, out string key)
        {
            key = null;
            foreach (var c in m_childContainers.Select(c => c.Value))
            {
                if (c.IsTypeRegistered(type))
                    key ??= c.ResolveKey(type);
            }

            return string.IsNullOrWhiteSpace(key);
        }

        private bool GetKeyFromParentContainer(Type type, out string key)
        {
            //Throw.If(string.IsNullOrWhiteSpace(key)).ArgumentNullException(nameof(key));

            key = null;
            if (m_parentContainer == null)
                return false;

            key ??= m_parentContainer?.ResolveKey(type);

            return string.IsNullOrWhiteSpace(key);
        }

        private object ResolveCore(Type type,
                                   string key = null,
                                   DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                   params object[] parameters)
        {
            m_lockSlim.TryEnter(SynchronizationAccess.Write);

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
                m_lockSlim.TryExit(SynchronizationAccess.Write);
            }
        }

        internal ShinDIContainer ResolveContainer(string key)
        {
            return this;
        }

        private bool GetTypeFromContainer(string key, out Type type)
        {
            type = null;
            return !string.IsNullOrWhiteSpace(key)
                && m_keyDictionary.TryGetValue(key, out type);
        }

        private bool GetTypeFromParentContainer(string key, out Type type)
        {
            type = null;
            m_lockSlim.TryEnter();
            try
            {
                if (string.IsNullOrWhiteSpace(key)
                 || m_parentContainer == null
                 || !m_parentContainer.IsKeyRegistered(key))
                    return false;

                type = m_parentContainer.ResolveType(key);
            }
            finally
            {
                m_lockSlim.TryExit();
            }

            return true;
        }

        private bool GetTypeFromChildContainer(string key, out Type type)
        {
            type = null;

            m_lockSlim.TryEnter();
            try
            {
                if (string.IsNullOrWhiteSpace(key)
                 || m_childContainers == null
                 || m_childContainers.IsEmpty)
                    return false;

                foreach (var c in m_childContainers.Select(c => c.Value))
                {
                    if (c.IsKeyRegistered(key))
                        type ??= c.ResolveType(key);

                    if (type != null)
                        break;
                }
            }
            finally
            {
                m_lockSlim.TryExit();
            }

            return true;
        }

        private void ResolveProperties(ref object instance,
                                       DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                       IDictionary<string, object> resolvedObjects = null)
        {
            var type = instance.GetType();
            ConcurrentList<PropertyInfo> injectableProperties;

            m_lockSlim.TryEnter();
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
                m_lockSlim.TryExit();
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

        private object BuildUp(Type type, string key, params object[] parameters)
        {
            if (type == null)
                throw new IoCResolutionException("type cannot not be null.");

            object instance = null;
            var cType = type;

            m_lockSlim.TryEnter();
            try
            {
                ResolverDictionary resolverDictionary;
                if (InterfaceCheck(type, out cType))
                {
                    key = ResolveKey(cType);
                    m_typeDictionary.TryGetValue(cType, out resolverDictionary);
                }
                else
                    m_typeDictionary.TryGetValue(type, out resolverDictionary);

                if (resolverDictionary != null &&
                    resolverDictionary.TryGetValue(key, out var resolver))
                    instance = resolver?.GetObject();
            }
            finally
            {
                m_lockSlim.TryExit();
            }

            if (cType != null)
                instance ??= Instantiate(type, cType, parameters);

            return instance;
        }

        private object[] BuildParameterList(ConstructorInvokeInfo info, params object[] parameters)
        {
            var constructorParameters = info.ParameterInfos;
            //var length = constructorParameters.Length;
            // var test = parameters.Length == length;
            var parametersList = new ConcurrentList<object>();
            // *test ? parameters : */ //new object[length];
            var ps = new ConcurrentList<object>(parameters);
            object parameter = null;

            foreach (var pi in constructorParameters)
            {
                if (constructorParameters.Length == parameters.Length)
                {
                    try
                    {
                        parameter = parameters[pi.Position];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        parameter = Resolve(pi.ParameterType);
                    }
                }

                while (parameter == null)
                {
                    foreach (var p in ps)
                    {
                        if (!p.GetType().ContainsType(pi.ParameterType))
                            continue;

                        parameter = p;
                        break;
                    }

                    parameter ??= Resolve(pi.ParameterType);

#if NET5_0_OR_GREATER
                    if (pi.HasDefaultValue)
                        parameter ??= pi.DefaultValue;
#endif
                }

                parametersList.Add(parameter);
                parameter = null;
            }

            return parametersList.ToArray();
        }

        private bool Instantiate(ConstructorInvokeInfo info, out object result, params object[] parameters)
        {
            result = null;
            var pl = BuildParameterList(info, parameters);
            var constructorFunc = info.ConstructorFunc;
            try
            {
                result = constructorFunc(pl);
                return true;
            }
            catch
            {
                return false;
                //throw new IoCResolutionException("Failed to resolve " + info.Constructor.DeclaringType, ex);
            }
        }

        private object Instantiate(Type type, Type cType, params object[] parameters)
        {
            Throw.IfNullArgument(type);
            Throw.IfNullArgument(cType);
            Throw.If(cType.IsInterface || cType.IsAbstract)!.InvalidOperationException();

            m_constructorDictionaryLockSlim.TryEnter(SynchronizationAccess.Write);

            try
            {
                if (!m_constructorDictionary.TryGetValue(type, out var invokeInfo))
                    invokeInfo = BuildConstructorList(type, cType);

                foreach (var i in invokeInfo)
                {
                    if (Instantiate(i, out var result, parameters))
                        return result;
                }
            }
            finally
            {
                m_constructorDictionaryLockSlim.TryExit(SynchronizationAccess.Write);
            }

            return null;
        }

        private ConstructorInvokeInfo[] BuildConstructorList(Type type, Type cType)
        {
            //var cons = new ConcurrentList<ConstructorInvokeInfo>();
#if NETFX_CORE
            var constructors = cType.GetTypeInfo().DeclaredConstructors.Where(x => !x.IsStatic && x.IsPublic).ToArray();
#else
            var constructors = cType.GetConstructors(BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
#endif

            if (constructors == null || constructors.Length <= 0)
            {
                throw new IoCResolutionException(
                                                 "Could not locate a constructor for " + type.FullName);
            }

            //ConstructorInfo constructor = null;

            //if (constructors.Length == 0)
            //{
            //    if (cType.IsPrimitive ^ cType.IsValueType ^ (cType == typeof(string)))
            //        return null;
            //}

            //switch (constructors.Length)
            //{
            //    case 1:
            //        //constructor = constructors[0];
            //        cons.Add(constructor);
            //        break;
            //    case > 1:
            //        {
            //ConstructorInfo bestMatch = null;
            CreateConstructors(constructors, out var ci);
            //cons.AddRange(ci);
            //constructor ??= bestMatch;
            //break;
            //    }
            //default:
            //    {
            //#if !NETFX_CORE
            //                        //ConstructorInfo bestMatch = null;
            //                        //var biggestLength = -1;

            //                        constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
            //                        if (constructors.Length > 0)
            //                        {
            //                            AddConstructors(constructors, out var ci2);
            //                            //constructorInfo.GetCustomAttributes(typeof(InjectConstructorAttribute), false).Length > 0

            //                            //constructor ??= bestMatch;
            //                        }
            //#endif
            //break;
            //}
            //}

            //if (constructor == null)
            //{
            //    throw new IoCResolutionException(
            //                                     "Could not locate a constructor for " + type.FullName);
            //}

            m_constructorDictionary[type] = ci;
            return ci;
        }

        private int CreateConstructors(ConstructorInfo[] constructors, out ConstructorInvokeInfo[] infos)
        {
            var biggestLength = -1;
            var cons = new ConcurrentList<ConstructorInfo>();
            foreach (var ci in constructors)
            {
                if (!HasDependencyAttributes(ci))
                {
                    var length = ci.GetParameters().Length;
                    if (length <= biggestLength)
                        continue;

                    biggestLength = length;
                }

                //bestMatch = ci;
                cons.Add(ci);
            }

            infos = cons.Select(c => new ConstructorInvokeInfo(c)).ToArray();
            return biggestLength;
        }

        private bool HasDependencyAttributes(ConstructorInfo constructorInfo)
        {
            var dependencyAttributes = constructorInfo.GetCustomAttributes(typeof(InjectConstructorAttribute), false);
#if NETFX_CORE
                            var attributeCount = dependencyAttributes.Count();
#else
            var attributeCount = dependencyAttributes.Length;
#endif

            return attributeCount > 0;
        }

        //private string GetKeyValueOrDefault(string key = null)
        //{
        //    if (string.IsNullOrWhiteSpace(key))
        //        key = Guid.NewGuid().ToString(); //m_defaultKey;

        //    return key;
        //}

        private void CreateResolver(Type T, Type cT, Func<object[], object> getInstanceFunc, string key, bool asSingleton, bool overrideExisting)
        {
            m_lockSlim.TryEnter(SynchronizationAccess.Write);
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    key = ResolveKey(T);

                //key = GetKeyValueOrDefault(key) ?? key;
                if (overrideExisting ^ !m_typeDictionary.TryGetValue(T, out var resolverDictionary))
                    resolverDictionary = new ResolverDictionary();

                if (asSingleton && resolverDictionary.TryGetValue(key, out var r))
                    return;

                var resolver = new TypeResolver(cT, getInstanceFunc, asSingleton);
                resolverDictionary[key] = resolver;
                m_typeDictionary[T] = resolverDictionary;
                m_keyDictionary[key] = T;
            }
            finally
            {
                m_lockSlim.TryExit(SynchronizationAccess.Write);
            }
        }

        private void CreateResolver<T, C>(Func<object[], object> getInstanceFunc, string key, bool asSingleton, bool overrideExisting)
        {
            CreateResolver(typeof(T), typeof(C), getInstanceFunc, key, asSingleton, overrideExisting);
        }

        private void CreateResolver<T>(Type cT, Func<object[], object> getInstanceFunc, string key, bool asSingleton, bool overrideExisting)
        {
            CreateResolver(typeof(T), cT, getInstanceFunc, key, asSingleton, overrideExisting);
        }

        private void CreateResolver<T>(Func<object[], object> getInstanceFunc, string key, bool asSingleton, bool overrideExisting)
        {
            CreateResolver(typeof(T), typeof(T), getInstanceFunc, key, asSingleton, overrideExisting);
        }

        private void CreateResolver<T>(Func<object> getInstanceFunc, string key, bool asSingleton, bool overrideExisting)
        {
            CreateResolver(typeof(T), getInstanceFunc, key, asSingleton, overrideExisting);
        }

        private void CreateResolver(Type T, Func<object> getInstanceFunc, string key, bool asSingleton, bool overrideExisting)
        {
            CreateResolver(T, T, p => getInstanceFunc(), key, asSingleton, overrideExisting);
        }

        private bool InterfaceCheck(Type iface, out Type concrete)
        {
            Throw.IfNull(iface).ArgumentNullException(nameof(iface));

            concrete = null;

            if (!iface.IsInterface)
            {
                concrete = iface;
                return false;
            }

            m_lockSlim.TryEnter();
            try
            {
                var cached = m_interfaceDictionary.ContainsKey(iface);
                if (cached)
                {
                    concrete = m_interfaceDictionary[iface][0];
                    return true;
                }

                foreach (var tr in m_typeDictionary)
                {
                    var t = tr.Key;
                    var resolvers = tr.Value;
                    if (resolvers is null)
                        continue;

                    foreach (var resolver in resolvers.Values)
                    {
                        var rt = resolver?.Type;
                        rt ??= t;

                        Throw.IfNull(rt).InvalidOperationException();

                        var interfaces = new ConcurrentList<Type>();
                        if (rt.IsInterface)
                            interfaces.Add(rt);
                        interfaces.AddRange(rt.GetInterfaces());

                        foreach (var i in interfaces)
                        {
                            if (i != iface)
                                continue;

                            if (m_interfaceDictionary.ContainsKey(iface) &&
                                !m_interfaceDictionary[iface].Contains(rt))
                                m_interfaceDictionary[iface].Add(rt);
                            else
                            {
                                m_interfaceDictionary.TryAdd(iface,
                                                             new ConcurrentList<Type> {rt});
                            }

                            concrete = rt;
                            break;
                        }
                    }
                }
            }
            finally
            {
                m_lockSlim.TryExit();
            }

            return concrete != null;

            //result = ResolveCore(match, key, strategy, parameters);
        }
        #endregion
    }
}