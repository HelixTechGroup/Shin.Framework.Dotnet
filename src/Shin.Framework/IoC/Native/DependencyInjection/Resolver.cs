#region Usings
using System;
using System.Linq;
using System.Reflection;
using Shin.Framework.Collections.Concurrent;
using Shin.Framework.Extensions;
using Shin.Framework.IoC.DependencyInjection;
using Shin.Framework.IoC.Native.DependencyInjection.Exceptions;
#endregion

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    internal partial class Resolver : Initializable,
                                      IResolver
    {
        #region Members
        //protected static readonly ReaderWriterLockSlim m_lockSlim = new(LockRecursionPolicy.SupportsRecursion);
        protected readonly Func<object[], object> m_createInstanceFunc;

        protected readonly BindingFlags m_defaultBindings = BindingFlags.Instance |
                                                            BindingFlags.IgnoreCase |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Public;

        protected readonly bool m_singleton;
        protected readonly Type m_type;
        protected ConcurrentList<ConstructorInvokeInfo> m_constructors;
        protected IDIContainer m_container;
        protected bool m_hasInstance;
        protected object m_instance;
        protected int m_instantCount;
        protected ConcurrentHashSet<Type> m_interfaces;
        #endregion

        #region Properties
        /// <inheritdoc />
        public bool HasInstance
        {
            get { return m_hasInstance; }
        }

        /// <inheritdoc />
        public Type[] Interfaces
        {
            get { return m_interfaces.ToArray(); }
        }

        public bool Singleton
        {
            get { return m_singleton; }
        }

        /// <inheritdoc />
        public Type Type
        {
            get { return m_type; }
        }
        #endregion

        //public Resolver() { }

        public Resolver(IDIContainer container,
                        Type type,
                        bool singleton,
                        Func<object[], object> instanceFunc) : this(container, type, singleton)
        {
            m_createInstanceFunc = instanceFunc;
        }

        public Resolver(IDIContainer container,
                        Type type,
                        bool singleton,
                        object instanceObject) : this(container, type, singleton)
        {
            m_instance = instanceObject;
        }
        
        public Resolver(IDIContainer container,
                        Type type,
                        bool singleton)
        {
            //Throw.If(type.IsInterface || type.IsAbstract)
            //     .ArgumentException(nameof(type));
            Throw.IfNullArgument(container);

            m_type         = type;
            m_singleton    = singleton;
            m_container    = container;
            m_interfaces   = new ConcurrentHashSet<Type>();
            m_constructors = new ConcurrentList<ConstructorInvokeInfo>();
        }

        #region Methods
        /// <inheritdoc />
        public bool CheckInterface(Type interfaceType)
        {
            Throw.IfNullArgument(interfaceType);
            Throw.IfNull(m_type)
                 .InvalidOperationException();
            //concrete = null;

            if (!interfaceType.IsInterface)
            {
                //concrete = new ConcurrentList<Type>(interfaceType);
                return false;
            }

            //m_lockSlim.TryEnter();
            lock(m_lock)
            {
                var cached = m_interfaces.Contains(interfaceType);
                if (cached)
                {
                    //concrete = new ConcurrentList<Type>(m_interfaceDictionary[interfaceType]);
                    return true;
                }

                //m_interfaces ??= new ConcurrentList<Type>();
                //var interfaces = new ConcurrentList<Type>();
                //interfaces.Add(interfaceType);
                //interfaces.AddRange(interfaceType.GetInterfaces());
                //interfaceType.GetInterfaces();
            }

            return false;
        }

        /// <inheritdoc />
        public bool CheckInterface<T>() { return CheckInterface(typeof(T)); }

        public object GetObject(params object[] parameters)
        {
            //m_lockSlim.TryEnter(SynchronizationAccess.Write);
            lock(m_lock) //try
            {
                if (m_singleton)
                {
                    m_instance     ??= CreateObject(parameters);
                    m_instantCount =   1;
                    return m_instance;
                }

                var instance = CreateObject(parameters);
                m_instantCount++;
                return instance;
            }
            //finally
            //{
            //    m_lockSlim.TryExit(SynchronizationAccess.Write);
            //}
        }

        protected override void DisposeManagedResources()
        {
            m_instance     = null;
            m_hasInstance  = false;
            m_instantCount = 0;
        }

        /// <inheritdoc />
        protected override void InitializeResources()
        {
            BuildInterfaceList();

            if (m_createInstanceFunc is null) BuildConstructorList();
        }

        protected void BuildInterfaceList()
        {
            var top = m_type.GetInterfaces();
            m_interfaces.AddRange(top);
            foreach (var tI in top) m_interfaces.AddRange(tI.GetInterfaces());
        }

        protected object CreateObject(params object[] parameters)
        {
            //m_lockSlim.TryEnter(SynchronizationAccess.Write);
            //try
            //{
            if (m_createInstanceFunc is not null) 
                return m_createInstanceFunc(parameters);

            if (m_type.IsValueType) return Activator.CreateInstance(m_type);

            foreach (var info in m_constructors)
            {
                var param = BuildParameterList(info, parameters);
                var result = info.Invoke(param);

                if (result == null) continue;

                return result;
            }
            //}
            //finally
            //{
            //    m_lockSlim.TryExit(SynchronizationAccess.Write);
            //}

            throw new IoCResolutionException();
        }

        protected object[] BuildParameterList(ConstructorInvokeInfo constructor,
                                              params object[] parameters)
        {
            var parametersList = new ConcurrentList<object>();
            var currentMatch = constructor.ParameterInfos.Length == parameters.Length;
            var constructorParameters = constructor.ParameterInfos;
            //var length = constructorParameters.Length;
            // var test = parameters.Length == length;
            // *test ? parameters : */ //new object[length];
            var ps = new ConcurrentList<object>(parameters);
            object parameter = null;

            foreach (var pi in constructorParameters)
            {
                //if (constructorParameters.Length == parameters.Length)
                //{
                //if (currentMatch)
                //{
                //    try
                //    {
                //        var tmp = parameters[pi.Position];
                //        if (pi.ParameterType == tmp.GetType())
                //            parameter = tmp;
                //        tmp = null;
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e);
                //        //parameter = ResolveAux(pi.ParameterType);
                //    }   
                //}
                //}

                //if (parameter == null)
                //{
                foreach (var p in ps)
                {
                    if (!p.GetType()
                          .ContainsType(pi.ParameterType))
                        continue;

                    parameter = p;
                    break;
                }

                ps.Remove(parameter);
                //}

#if NET5_0_OR_GREATER
                if (pi.HasDefaultValue) parameter ??= pi.DefaultValue;
#endif

                parameter ??= m_container.Resolve(pi.ParameterType);

                if (parameter is null) continue;

                parametersList.Add(parameter);
                parameter = null;
            }

            //Throw.If<IoCResolutionException>(!ps.IsEmpty());
            return parametersList.ToArray();
        }

        protected void BuildConstructorList()
        {
            if (m_type.IsInterface ||
                m_type.IsAbstract)
                return;

            //var cons = new ConcurrentList<ConstructorInvokeInfo>();
#if NETFX_CORE
            var constructors = cType.GetTypeInfo()
                                    .DeclaredConstructors.Where(x => !x.IsStatic && x.IsPublic)
                                    .ToArray();
#else
            var constructors = m_type.GetConstructors(m_defaultBindings);
#endif

            //if (cType.IsValueType)
            //    constructors = new ConstructorInfo[] {cType.TypeInitializer};

            if (constructors.Length <= 0) throw new IoCResolutionException("Could not locate a constructor for " + m_type.FullName);

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
                                         out ConstructorInvokeInfo[] infos)
        {
            var biggestLength = -1;
            var cons = new ConcurrentList<ConstructorInfo>();
            foreach (var ci in constructors)
            {
                if (!HasDependencyAttributes(ci))
                {
                    var length = ci.GetParameters()
                                   .Length;
                    if (length <= biggestLength) continue;

                    biggestLength = length;
                }

                //bestMatch = ci;
                cons.Add(ci);
            }

            infos = cons.Select(c => new ConstructorInvokeInfo(c))
                        .ToArray();
            return biggestLength;
        }

        private static bool HasDependencyAttributes(ConstructorInfo constructorInfo)
        {
            var dependencyAttributes = constructorInfo.GetCustomAttributes(typeof(InjectConstructorAttribute), false);
#if NETFX_CORE
            var attributeCount = dependencyAttributes.Count();
#else
            var attributeCount = dependencyAttributes.Length;
#endif

            return attributeCount > 0;
        }
        #endregion
    }

    internal class Resolver<T> : Resolver,
                                 IResolver<T>
    {
        /// <inheritdoc />
        public Resolver(IDIContainer container,
                        Type type,
                        bool singleton) : base(container, type, singleton) { }

        #region Methods
        /// <inheritdoc />
        public new T GetObject(params object[] parameters) { return (T) base.GetObject(parameters); }
        #endregion
    }
}