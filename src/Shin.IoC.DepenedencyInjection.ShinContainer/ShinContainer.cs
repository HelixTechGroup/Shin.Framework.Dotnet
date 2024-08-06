#region Usings
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Shin.Collections.Concurrent;
using Shin.Extensions;
using Shin.IoC.DependencyInjection;
using Shin.IoC.DependencyInjection.Exceptions;
using Shin.IoC.DependencyInjection.Registration.Collections;
using Shin.IoC.DependencyInjection.Runtime.Activation;
using Shin.IoC.DependencyInjection.Runtime.Activation.Collections;
using Shin.IoC.DependencyInjection.Runtime.Caching;
using Shin.Threading;

//using System.Reflection.Metadata;
#endregion

namespace Shin.IoC.DependencyInjection
{
    public sealed partial class ShinContainer : Disposable,
                                                  IDIContainer,
                                                  IDIChildContainer,
                                                  IDIParentContainer,
                                                  IDIRootContainer
    {
        #region Members
        private static readonly ReaderWriterLockSlim m_buildLockSlim = new();
        //private static readonly ReaderWriterLockSlim m_constructorDictionaryLockSlim = new();
        private static readonly ReaderWriterLockSlim m_lockSlim = new(LockRecursionPolicy.SupportsRecursion);
        private static readonly ReaderWriterLockSlim m_typeDictionaryLockSlim = new(LockRecursionPolicy.SupportsRecursion);

        private readonly ConcurrentDictionary<string, Guid> m_keyDictionary;
        private readonly TypeResolverDictionary m_typeDictionary;
        private readonly TypeRegistrationDictionary m_registrationDictionary;
        private readonly IInterfaceMappingCache m_interfaceCache;
        //private readonly ConcurrentDictionary<Guid, ConcurrentHashSet<Guid>> m_interfaceDictionary;

        private readonly ConcurrentDictionary<string, IDIChildContainer> m_childContainers;

        //private readonly ConcurrentDictionary<Type, ConstructorInvokeInfo[]> m_constructorDictionary;
        //private readonly ConcurrentDictionary<Type, ConcurrentList<string>> m_cycleDictionary;
        private readonly string m_defaultKey;
        private readonly Guid m_id;
        //private readonly ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>> m_injectablePropertyDictionary;
        private readonly IDIParentContainer m_parentContainer;
        //private readonly ConcurrentDictionary<string, Action<object, object>> m_propertyActionDictionary;
        //private readonly ConcurrentDictionary<Type, PropertyInfo[]> m_propertyDictionary;
        private readonly IDIRootContainer m_root;
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
            get { return m_typeDictionary.RegisteredTypes; }
        }

        IEnumerable<Type> IDIContainer.RegisteredInterfaces
        {
            get { return m_interfaceCache.Interfaces; }
        }
        #endregion

        public ShinContainer(IDIContainer parent) : this((IDIParentContainer) parent) { }

        public ShinContainer()
        {
            m_id = Guid.NewGuid();

            m_keyDictionary = new ConcurrentDictionary<string, Guid>();

            m_typeDictionary = new TypeResolverDictionary(); //new ConcurrentDictionary<Guid, TypeResolverDictionary>();
            //m_interfaceDictionary = new ConcurrentDictionary<Guid, ConcurrentHashSet<Guid>>();
            m_interfaceCache = new TypeInterfaceCache(this);

            //m_defaultKey = Guid.NewGuid()
            //.ToString();
            //m_lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            //m_constructorDictionaryLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            //m_cycleDictionary = new ConcurrentDictionary<Type, ConcurrentList<string>>();
            //m_constructorDictionary        =   new ConcurrentDictionary<Type, ConstructorInvokeInfo[]>();
            //m_injectablePropertyDictionary = new ConcurrentDictionary<Type, ConcurrentList<PropertyInfo>>();
            //m_propertyActionDictionary = new ConcurrentDictionary<string, Action<object, object>>();
            //m_propertyDictionary = new ConcurrentDictionary<Type, PropertyInfo[]>();
            m_childContainers = new ConcurrentDictionary<string, IDIChildContainer>();
            m_root ??= this;
            //m_parentContainer ??= this;

            Register<IDIContainer>(this);
            Register<IDIRootContainer>(this);
            Register(() => ((IDIParentContainer) this).CreateChildContainer(), false);
            //CreateResolver(((IDIParentContainer)this).CreateChildContainer, null, false, false);
        }

        internal ShinContainer(IDIParentContainer parent) : this()
        {
            Throw.IfNull(parent)
                 .ArgumentNullException(nameof(parent));

            m_parentContainer = parent;
            m_root = m_parentContainer?.Root;
            Register(m_parentContainer);
        }

        #region Methods
        public void Load(params IBindings[] bindings) { throw new NotImplementedException(); }

        public void Unload(params IBindings[] bindings) { throw new NotImplementedException(); }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            sb.AppendLine(@$"Container Id: {m_id}");
            sb.AppendLine($@"Root Container: {m_root?.Id}");
            sb.AppendLine($@"Parent Container: {m_parentContainer?.Id}");
            sb.AppendLine(@$"Type Dictionary Count: {m_typeDictionary.Count}");
            foreach (var t in m_typeDictionary)
            {
                sb.AppendLine(@$"Type key: {t.Key}");
                foreach (var v in t.Value.Values) sb.AppendLine(@$"Type Value: {v.Type.Name}");
            }

            sb.AppendLine();
            sb.AppendLine(@$"Interface Dictionary Count: {m_interfaceCache?.Count}");
            foreach (var t in m_interfaceCache)
            {
                sb.AppendLine(@$"Type key: {t.Key.Name}");
                foreach (var v in t.Value) sb.AppendLine(@$"Type Value: {v}");
            }

            sb.AppendLine();
            sb.AppendLine(@"Child Containers:");
            foreach (var child in m_childContainers)
                sb.AppendLine($@"{child.Value}");

            return sb.ToString();
        }

        protected override void DisposeManagedResources()
        {
            Release();
        }

        IDIChildContainer IDIParentContainer.CreateChildContainer()
        {
            var container = new ShinContainer(this);
            m_childContainers[container.Id.ToString()] = container;
            return container;
        }

        private bool GetKeyFromContainer(Type type,
                                         out string key)
        {
            //Throw.If(type is null)
            //     .ArgumentNullException(nameof(type));

            key = null;
            //if (type.IsInterface)
            //{
            //    foreach (var k in m_keyDictionary)
            //        if (k.Value.ContainsInterface(type))
            //            key = k.Key;
            //}


            //if (m_keyDictionary.Values.Contains(type))
            //{
            //    foreach (var k in m_keyDictionary)
            //        if (k.Value == type)
            //            key = k.Key;
            //}

            //if (!string.IsNullOrWhiteSpace(key)) return true;

            //key = type.GUID.ToString();
            return false;
        }

        private bool GetKeyFromChildContainer(Type type,
                                              out string key)
        {
            key = null;
            //foreach (var c in m_childContainers.Select(c => c.Value))
            //    if (c.IsTypeRegistered(type))
            //        key ??= ResolveTypeId(type);

            return string.IsNullOrWhiteSpace(key);
        }

        private bool GetKeyFromParentContainer(Type type,
                                               out string key)
        {
            //Throw.If(string.IsNullOrWhiteSpace(key)).ArgumentNullException(nameof(key));

            key = null;
            if (m_parentContainer == null) return false;

            //key ??= ResolveTypeId(type);

            return string.IsNullOrWhiteSpace(key);
        }

        private bool GetTypeFromContainer(string key,
                                          out Type type)
        {
            type = null;
            return !string.IsNullOrWhiteSpace(key) &&
                   m_keyDictionary.TryGetValue(key, out type);
        }

        //private bool GetTypeFromParentContainer(string key,
        //                                        out Type type)
        //{
        //    type = null;
        //    //m_lockSlim.TryEnter();
        //    if (string.IsNullOrWhiteSpace(key) ||
        //        m_parentContainer == null ||
        //        !m_parentContainer.IsKeyRegistered(key))
        //        return false;

        //    type = m_parentContainer.ResolveType(key);

        //    return true;
        //}

        //private bool GetTypeFromChildContainer(string key,
        //                                       out Type type)
        //{
        //    type = null;

        //    //m_lockSlim.TryEnter();
        //    if (string.IsNullOrWhiteSpace(key) ||
        //        m_childContainers == null ||
        //        m_childContainers.IsEmpty)
        //        return false;

        //    foreach (var c in m_childContainers.Select(c => c.Value))
        //    {
        //        if (c.IsKeyRegistered(key)) type ??= c.ResolveType(key);

        //        if (type != null) break;
        //    }

        //    return true;
        //}

        private object BuildUp(Type type,
                               string key,
                               params object[] parameters)
        {
            if (type == null) throw new IoCResolutionException("type cannot not be null.");

            //object instance = null;
            TypeResolverDictionary resolverDictionary = null;
            //var cType = type;

            m_buildLockSlim.TryEnter(SynchronizationAccess.Write);
            try
            {
                var typeId = type.GUID;//ResolveTypeId(type);
                //key ??= typeId;
                //var cT = type;
                ITypeResolver resolver = null;
                //if (!m_interfaceCache.IsRegistered(type, out var cType))
                //m_typeDictionary.TryGetValue(typeId, out resolver))
                //else
                //{
                //    foreach (var c in cType)
                //    {
                //        var cTypeId = ResolveTypeId(c);
                //        if (m_typeDictionary.TryGetValue(cTypeId, out resolverDictionary)) 
                //            break;

                //        //break;
                //    }
                //}

                //if (resolverDictionary == null)
                if (!m_typeDictionary.TryGetValue(typeId, out resolver))
                {
                    resolver = CreateResolver(type,
                                       type,
                                       key);

                    m_typeDictionary.TryAdd(typeId, resolver);
                }

                return resolver?.Construct(parameters);
            }
            finally
            {
                m_buildLockSlim.TryExit(SynchronizationAccess.Write);
            }

            throw new IoCResolutionException();
        }

        //private bool Instantiate(ConstructorInvokeInfo info,
        //                         out object result,
        //                         params object[] parameters)
        //{
        //    result = null;
        //    try
        //    {
        //        var pl = BuildParameterList(info, parameters);
        //        result = info.Invoke(pl);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //        //throw new IoCResolutionException("Failed to resolve " + info.Constructor.DeclaringType, ex);
        //    }
        //}

        //private object Instantiate(Type type,
        //                           Type cType,
        //                           params object[] parameters)
        //{
        //    Throw.IfNullArgument(type);
        //    Throw.IfNullArgument(cType);
        //    Throw.If(cType.IsInterface || cType.IsAbstract)!.InvalidOperationException();

        //    //m_constructorDictionaryLockSlim.TryEnter(SynchronizationAccess.Write);

        //    //try
        //    //{
        //        if (!m_typeDictionary.TryGetValue(type, out var invokeInfo)) 
        //            invokeInfo = BuildConstructorList(type, cType);

        //        foreach (var i in invokeInfo)
        //        {
        //            if (Instantiate(i, out var result, parameters)) return result;
        //        }

        //        if (type.IsValueType) return Activator.CreateInstance(cType);

        //        Throw.Exception<IoCResolutionException>(@$"Failed to resolve {type.FullName}");
        //    //}
        //    //finally
        //    //{
        //    //    m_constructorDictionaryLockSlim.TryExit(SynchronizationAccess.Write);
        //    //}

        //    return null;
        //}

        //private string GetKeyValueOrDefault(string key = null)
        //{
        //    if (string.IsNullOrWhiteSpace(key))
        //        key = Guid.NewGuid().ToString(); //m_defaultKey;

        //    return key;
        //}
        #endregion

        //private static Func<string, T> CreateInstanceFunc<T>()
        //{
        //    var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        //    var ctor = typeof(T).GetConstructors(flags).Single(
        //                                                       ctors =>
        //                                                       {
        //                                                           var parameters = ctors.GetParameters();
        //                                                           return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
        //                                                       });
        //    var value = Expression.Parameter(typeof(string), "value");
        //    var body = Expression.New(ctor, value);
        //    var lambda = Expression.Lambda<Func<string, T>>(body, value);

        //    return lambda.Compile();
        //}
    }
}