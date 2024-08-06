#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Shin.Collections.Concurrent;
using Shin.Extensions;
using Shin.IoC.DependencyInjection;
using Shin.IoC.DependencyInjection.Exceptions;
using Shin.IoC.DependencyInjection.Runtime.Activation.Injectors;
#endregion

namespace Shin.IoC.DependencyInjection.Runtime.Activation
{
    internal class TypeResolver : Initializable,
                              ITypeResolver
    {
        #region Members
        //protected static readonly ReaderWriterLockSlim m_lockSlim = new(LockRecursionPolicy.SupportsRecursion);
        protected static Func<object[], object> m_createInstanceFunc;

        protected readonly BindingFlags m_defaultBindings = BindingFlags.Instance |
                                                            BindingFlags.IgnoreCase |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Public;

        //protected readonly bool m_singleton;
        protected readonly Type m_type;
        protected ConcurrentList<ConstructorInjector> m_constructors;
        protected IDIContainer m_container;
        //protected bool m_hasInstance;
        //protected object m_instance;
        //protected int m_instantCount;
        protected ConcurrentHashSet<Guid> m_interfaces;
        protected ConcurrentList<PropertyInjector> m_properties;
        #endregion

        #region Properties
        /// <inheritdoc />
        //public bool HasInstance
        //{
        //    get { return m_hasInstance; }
        //}

        public Guid Id
        {
            get { return m_type?.GUID ?? Guid.Empty; }
        }

        /// <inheritdoc />
        //public Type[] Interfaces
        //{
        //    get { return m_interfaces.Select(Type.GetTypeFromCLSID).ToArray(); }
        //}

        //public IReadOnlyCollection<Guid> Interfaces
        //{
        //    get { return m_interfaces; }
        //}

        //public bool Singleton
        //{
        //    get { return m_singleton; }
        //}

        /// <inheritdoc />
        public Type Type
        {
            get { return m_type; }
        }
        #endregion

        //public Resolver() { }

        public TypeResolver(IDIContainer container,
                        Type type,
                        //bool singleton,
                        Func<object[], object> instanceFunc) : this(container, type/*, singleton*/)
        {
            m_createInstanceFunc = instanceFunc;
            //m_instance           = instanceFunc;
        }

        //public TypeResolver(IDIContainer container,
        //                Type type,
        //                object instanceObject) : this(container, type, true)
        //{
        //    m_instance = instanceObject;
        //}

        public TypeResolver(IDIContainer container,
                        Type type,
                        /*bool singleton*/)
        {
            //Throw.If(type.IsInterface || type.IsAbstract)
            //     .ArgumentException(nameof(type));
            Throw.IfNullArgument(container);

            m_type = type;
            //m_singleton    = singleton;
            m_container = container;
            m_interfaces = new ConcurrentHashSet<Guid>();
            m_constructors = new ConcurrentList<ConstructorInjector>();
            m_properties = new ConcurrentList<PropertyInjector>();
        }

        #region Methods
        /// <inheritdoc />
        //public bool CheckInterface(Type interfaceType)
        //{
        //    Throw.IfNullArgument(interfaceType);
        //    Throw.IfNull(m_type)
        //         .InvalidOperationException();
        //    //concrete = null;

        //    if (!interfaceType.IsInterface)
        //    {
        //        //concrete = new ConcurrentList<Type>(interfaceType);
        //        return false;
        //    }

        //    //m_lockSlim.TryEnter();
        //    lock(m_lock)
        //    {
        //        var cached = m_interfaces.Contains(interfaceType.GUID);
        //        if (cached)
        //        {
        //            //concrete = new ConcurrentList<Type>(m_interfaceDictionary[interfaceType]);
        //            return true;
        //        }

        //        //m_interfaces ??= new ConcurrentList<Type>();
        //        //var interfaces = new ConcurrentList<Type>();
        //        //interfaces.Add(interfaceType);
        //        //interfaces.AddRange(interfaceType.GetInterfaces());
        //        //interfaceType.GetInterfaces();
        //    }

        //    return false;
        //}

        ///// <inheritdoc />
        //public bool CheckInterface<T>() { return CheckInterface(typeof(T)); }

        public ITypeInstance Construct(params object[] parameters)
        {
            //m_lockSlim.TryEnter(SynchronizationAccess.Write);
            lock (m_lock) //try
            {
                //if (m_singleton)
                //{
                //    m_instance     ??= CreateObject(parameters);
                //    m_instantCount =   1;
                //    return m_instance;
                //}

                var instance = CreateInstance(parameters);
                //m_instantCount++;
                return instance;
            }
            //finally
            //{
            //    m_lockSlim.TryExit(SynchronizationAccess.Write);
            //}
        }

        protected override void DisposeManagedResources()
        {
            //((IDisposable) m_instance)?.Dispose();
            //m_instance     = null;
            //m_hasInstance  = false;
            //m_instantCount = 0;
        }

        /// <inheritdoc />
        protected override void InitializeResources()
        {
            BuildInterfaceList();

            if (m_createInstanceFunc is null /*| m_instance is null*/)
                BuildConstructorList();

            BuildPropertyList();
            BuildMethodList();
        }

        private void BuildMethodList() { }

        private void BuildPropertyList() { }

        protected void BuildInterfaceList()
        {
            var top = m_type.GetInterfaces();
            m_interfaces.AddRange(top.Select(t => t.GUID));
            foreach (var tI in top)
            {
                m_interfaces.AddRange(tI.GetInterfaces()
                                        .Select(i => i.GUID));
            }
        }

        protected ITypeInstance CreateInstance(params object[] parameters)
        {
            //m_lockSlim.TryEnter(SynchronizationAccess.Write);
            //try
            //{
            if (m_createInstanceFunc is not null) return m_createInstanceFunc(parameters);

            if (m_type.IsValueType) return Activator.CreateInstance(m_type);

            foreach (var info in m_constructors)
            {
                //var param = BuildParameterList(info, parameters);
                var result = info.Inject(parameters);

                if (result != null) return result;
            }
            //}
            //finally
            //{
            //    m_lockSlim.TryExit(SynchronizationAccess.Write);
            //}

            throw new IoCResolutionException();
        }



        protected void BuildConstructorList()
        {
            if (m_type.IsInterface ||
                m_type.IsAbstract)
                throw new IoCResolutionException("Type cannot be abstract or an interface: " + m_type.FullName);

            //var cons = new ConcurrentList<ConstructorInvokeInfo>();

            var constructors = m_type.GetConstructors(m_defaultBindings);

            //if (cType.IsValueType)
            //    constructors = new ConstructorInfo[] {cType.TypeInitializer};

            if (constructors.Length <= 0)
                throw new IoCResolutionException("Could not locate a constructor for " + m_type.FullName);

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

            m_constructors.AddRange(ci);
        }

        protected int CreateConstructors(ConstructorInfo[] constructors,
                                         out ConstructorInjector[] infos)
        {
            var biggestLength = -1;
            var cons = new ConcurrentList<ConstructorInfo>();
            foreach (var ci in constructors)
            {
                //if (HasDependencyAttributes(ci))
                {
                    var length = ci.GetParameters()
                                   .Length;
                    if (length <= biggestLength) continue;

                    biggestLength = length;
                }

                cons.Add(ci);
            }

            infos = cons.Select(c => new ConstructorInjector(m_constructors, c))
                        .ToArray();
            return biggestLength;
        }

        private static bool HasDependencyAttributes(ConstructorInfo constructorInfo)
        {
            var dependencyAttributes = constructorInfo.GetCustomAttributes(typeof(InjectAttribute), false);
            var attributeCount = dependencyAttributes.Length;

            return attributeCount > 0;
        }

        internal struct TypeInstance : ITypeInstance
        {
            private ITypeResolver m_resolver;
            private object m_value;

            /// <inheritdoc />
            public ITypeResolver Resolver
            {
                get { return m_resolver; }
            }

            /// <inheritdoc />
            public object Value
            {
                get { return m_value; }
            }

            public TypeInstance(ITypeResolver resolver,
                                object value)
            {
                m_resolver = resolver;
                m_value = value;
            }

            /// <inheritdoc />
            public void Inject(string propertyName,
                               object value)
            { }

            /// <inheritdoc />
            public void Inject(string methodName,
                               params object[] arguments)
            { }
        }
        #endregion
    }

    //internal class Resolver<T> : Resolver,
    //                             ITypeResolver<T>
    //{
    //    /// <inheritdoc />
    //    public Resolver(IDIContainer container,
    //                    Type type,
    //                    bool singleton,
    //                    Func<object[], object> instanceFunc) : base(container, type, singleton, instanceFunc) { }

    //    /// <inheritdoc />
    //    public Resolver(IDIContainer container,
    //                    Type type,
    //                    object instanceObject) : base(container, type, instanceObject) { }

    //    /// <inheritdoc />
    //    public Resolver(IDIContainer container,
    //                    Type type,
    //                    bool singleton) : base(container, type, singleton) { }

    //    #region Methods
    //    /// <inheritdoc />
    //    public T GetInstance(params object[] parameters) { return (T) base.GetObject(parameters); }
    //    #endregion
    //}
}