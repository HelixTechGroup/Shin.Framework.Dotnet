#region Usings
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Shinject;
using Shinject.Activation;
using Shinject.Activation.Caching;
using Shinject.Components;
using Shinject.Infrastructure;
using Shinject.Parameters;
using Shinject.Planning;
using Shinject.Planning.Bindings;

using Shin.Collections.Concurrent;
using Shinject.Extensions;
using Shinject.Syntax;
#endregion

namespace Shin.IoC.DependencyInjection
{
    public sealed class ShinjectContainer : Disposable,
                                           IDIShinjectContainer
    {
        #region Members
        private readonly ConcurrentDictionary<Guid, HashSet<string[]>> m_constructorArgumentCache;


        private readonly IRootKernel m_kernel;
        #endregion

        #region Properties
        /// <inheritdoc />
        public Guid Id
        {
            get { return m_kernel.Id; }
        }

        /// <inheritdoc />
        public IEnumerable<Type> RegisteredInterfaces
        {
            get
            {
                return GetAllBindings()
                      .Where(b => b.Key.IsInterface)
                      .Select(b => b.Key);
            }
        }

        /// <inheritdoc />
        public IEnumerable<Type> RegisteredTypes
        {
            get
            {
                return GetAllBindings()
                      .Where(b => b.Key.IsClass)
                      .Select(b => b.Key);
            }
        }

        /// <inheritdoc />
        public IDIRootContainer Root
        {
            get { return m_kernel is IRootedKernel rootedKernel ? new ShinjectContainer(rootedKernel.Root) : this; }
        }

        /// <inheritdoc />
        IDIChildContainer[] IDIParentContainer.ChildContainers
        {
            get
            {
                return m_kernel.Children.Select(c => new ShinjectContainer((IRootedKernel) c))
                               .ToArray() ??
                       Array.Empty<IDIChildContainer>();
            }
        }

        /// <inheritdoc />
        IKernel IDIShinjectContainer.Kernel
        {
            get { return m_kernel; }
        }

        /// <inheritdoc />
        IDIParentContainer IDIChildContainer.ParentContainer
        {
            get { return m_kernel is IRootedKernel rootedKernel ? new ShinjectContainer((IRootKernel) rootedKernel.Parent) : this; }
        }
        #endregion

        public ShinjectContainer()
        {
            //m_id = Guid.NewGuid();
            m_kernel ??= new RootKernel();
            //var tmp = new StandardKernel();

            //m_root ??= m_parent is null ? this : m_parent.Root;
            //m_childContainers = new ConcurrentDictionary<Guid, IDIChildContainer>();
            //m_childTypeCache = new ConcurrentDictionary<Type, ConcurrentHashSet<Guid>>();
            m_constructorArgumentCache = new ConcurrentDictionary<Guid, HashSet<string[]>>();
            //m_parentContainer ??= this;

            //Register<IDIChildContainer>(() => CreateChildContainer(), false);

            //if (m_parent is null)
            //{
            //m_kernel.Bind(c => c.FromThisAssembly()
            //                     //.IncludingNonPublicTypes()
            //                    .SelectAllClasses()
            //                    .BindAllInterfaces());

            //m_kernel.Bind(c => c.From(Assembly.GetCallingAssembly()
            //                                  .GetReferencedAssemblies()
            //                                  .Select(a => a.FullName))
            //                     //.IncludingNonPublicTypes()
            //                    .SelectAllClasses()
            //                    .BindAllInterfaces());
            //}

            //m_kernel.Bind(c => c.FromThisAssembly()
            //                     //.IncludingNonPublicTypes()
            //                    .Select(t => !t.IsInterface && !t.IsAbstract &&
            //                                 t.GetInterfaces().Length == 0)
            //                    .BindToSelf());

            Register<IDIContainer>(this, overrideExisting: true);
            //Register(m_root, overrideExisting: true);

            m_kernel.Rebind<IDIChildContainer>()
                    .ToMethod(m => CreateChildContainer());
        }

        public ShinjectContainer(IDIShinjectContainer parent) : this()
        {
            //m_parent = parent;
            m_kernel = parent.Kernel.Get<IRootedKernel>(); //new RootedKernel(parent.Kernel as IParentKernel);
            //m_root = m_parent?.Root;
            //Register(m_parent, overrideExisting: true);
        }

        protected ShinjectContainer(IRootKernel kernel) { m_kernel = kernel; }

        #region Methods
        /// <inheritdoc />
        public void Register<T>(T value,
                                bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            RegisterCore(value, asSingleton, key, overrideExisting);
        }

        /// <inheritdoc />
        public void Register(object value,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            RegisterCore(value.GetType(),
                         value,
                         asSingleton,
                         key,
                         overrideExisting);
        }

        /// <inheritdoc />
        public void Register<T>(bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            RegisterCore(default(T?), asSingleton, key, overrideExisting);
        }

        /// <inheritdoc />
        public void Register(Type T,
                             object value,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            RegisterCore(value, asSingleton, key, overrideExisting);
        }

        /// <inheritdoc />
        public void Register(Type T,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            RegisterCore(T,
                         null,
                         asSingleton,
                         key,
                         overrideExisting);
        }

        /// <inheritdoc />
        public void Register<T>(Type C,
                                bool asSingleton = true,
                                string key = null,
                                bool overrideExisting = false)
        {
            RegisterCore(typeof(T),
                         C,
                         null,
                         asSingleton,
                         key,
                         overrideExisting);
        }

        /// <inheritdoc />
        public void Register<T, C>(C value,
                                   bool asSingleton = true,
                                   string key = null,
                                   bool overrideExisting = false)
            where C : class, T
        {
            RegisterCore<T, C>(value, asSingleton, key, overrideExisting);
        }

        /// <inheritdoc />
        public void Register<T, C>(bool asSingleton = true,
                                   string key = null,
                                   bool overrideExisting = false)
            where C : class, T
        {
            RegisterCore<T, C>(default, asSingleton, key, overrideExisting);
        }

        /// <inheritdoc />
        public void Register(Type T,
                             Type C,
                             bool asSingleton = true,
                             string key = null,
                             bool overrideExisting = false)
        {
            RegisterCore(T,
                         C,
                         null,
                         asSingleton,
                         key,
                         overrideExisting);
        }

        /// <inheritdoc />
        public void Unregister<T>(string key = null) { throw new NotImplementedException(); }

        /// <inheritdoc />
        public void Unregister(Type T,
                               string key = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Unregister(object value) { throw new NotImplementedException(); }

        /// <inheritdoc />
        public void UnregisterAll(Type T) { throw new NotImplementedException(); }

        /// <inheritdoc />
        public void UnregisterAll<T>() { throw new NotImplementedException(); }

        /// <inheritdoc />
        public bool IsKeyRegistered(string key,
                                    DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            return false;
        }

        /// <inheritdoc />
        public bool IsTypeRegistered<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default) { return m_kernel.CanResolve<T>(); }

        /// <inheritdoc />
        public bool IsTypeRegistered(Type T,
                                     DIResolutionStrategy strategy = DIResolutionStrategy.Default)
        {
            //if (!ChildTypeCheck(T, out var c)) 
            return m_kernel.CanResolve(T);

            //return false;
        }

        /// <inheritdoc />
        public void Load(params IBindings[] bindings) { }

        /// <inheritdoc />
        public void Unload(params IBindings[] bindings) { }

        /// <inheritdoc />
        public void Release() { m_kernel.Dispose(); }

        /// <inheritdoc />
        public T Resolve<T>(string key = null,
                            DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                            params object[] parameters)
        {
            return (T) Resolve(typeof(T), key, strategy, parameters);
        }

        /// <inheritdoc />
        public object Resolve(Type T,
                              string key = null,
                              DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                              params object[] parameters)
        {
            CheckParameters(T, out var p, parameters);
            var result = m_kernel.Get(T, (RootedResolutionStrategy) strategy, p);
            //TraverseContainers(T, out var result, p);
            return result;
        }

        /// <inheritdoc />
        public IEnumerable<T> ResolveAll<T>(DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                            params object[] parameters)
        {
            return ResolveAll(typeof(T), strategy, parameters)
              ?.Cast<T>();
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveAll(Type T,
                                              DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                              params object[] parameters)
        {
            CheckParameters(T, out var ninjectParams, parameters);
            var result = m_kernel.GetAll(T, (RootedResolutionStrategy) strategy, ninjectParams);
            //TraverseContainersAll(T, out var result);
            return result;
        }

        /// <inheritdoc />
        public bool TryResolve<T>(out T result,
                                  string key = null,
                                  DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                  params object[] parameters)
        {
            result = default;
            var res = new object();
            try
            {
                //if (TraverseContainers(typeof(T), ref res))
                //{
                //    result = (T) res;
                //    return true;
                //}
                result = Resolve<T>(key, strategy, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
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
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
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
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryResolveAll<T>(out IEnumerable<T> result,
                                     DIResolutionStrategy strategy = DIResolutionStrategy.Default,
                                     params object[] parameters)
        {
            result = null;
            try
            {
                result = ResolveAll<T>(strategy, parameters);
                return result.Any();
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool IsTypeRegisteredByParent<T>() { throw new NotImplementedException(); }

        /// <inheritdoc />
        public bool IsTypeRegisteredByParent(Type T) { throw new NotImplementedException(); }

        /// <inheritdoc />
        public bool IsTypeRegisteredByChild<T>() { throw new NotImplementedException(); }

        /// <inheritdoc />
        public bool IsTypeRegisteredByChild(Type T) { throw new NotImplementedException(); }

        /// <inheritdoc />
        public IDIChildContainer CreateChildContainer()
        {
            var c = m_kernel.Get<IRootedKernel>();
            var container = new ShinjectContainer(c);
            //m_childContainers[container.Id] = container;
            return container;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            sb.AppendLine(@$"Container Id: {m_kernel.Id}");

            if (m_kernel is IRootedKernel rootKernel)
            {
                sb.AppendLine($@"Root Container: {rootKernel.Id}");
                sb.AppendLine($@"Parent Container: {rootKernel.Parent.Id}");
            }

            sb.AppendLine($@"Ninject Kernel: {m_kernel.ToString()}");
            //sb.AppendLine(@$"Type Dictionary Count: {m_typeDictionary.Count}");
            //foreach (var t in m_typeDictionary)
            //{
            //    sb.AppendLine(@$"Type key: {t.Key}");
            //    foreach (var v in t.Value.Values) sb.AppendLine(@$"Type Value: {v.Type.Name}");
            //}

            //sb.AppendLine();
            //sb.AppendLine(@$"Interface Dictionary Count: {m_interfaceCache?.Count}");
            //foreach (var t in m_interfaceCache)
            //{
            //    sb.AppendLine(@$"Type key: {t.Key.Name}");
            //    foreach (var v in t.Value) sb.AppendLine(@$"Type Value: {v}");
            //}

            sb.AppendLine();
            sb.AppendLine(@"Child Containers:");
            foreach (var child in m_kernel.Children) sb.AppendLine($@"{child.ToString()}");

            return sb.ToString();
        }

        //private bool ChildTypeCheck(Type type,
        //                            out ISet<Guid> container)
        //{
        //    container = null;
        //    var found = false;


        //    //if (CheckCache(type, ref container)) return true;

        //    found = CanResolve(type, out var list, m_childContainers.Values.ToArray());

        //    //if (found) Console.WriteLine(list);
        //    //foreach (var c in m_childContainers.Values)
        //    //{
        //    //    if (((IDINinjectContainer) c).Kernel.CanResolve(type))
        //    //    {
        //    //        cl.Add(c.Id);
        //    //        found = true;
        //    //        break;
        //    //    }

        //    //    if (found) continue;

        //    //    foreach (var cc in ((IDIParentContainer) c).ChildContainers)
        //    //    {
        //    //        if (CanResolve())
        //    //        {
        //    //            cl.Add(c.Id);
        //    //            found = true;
        //    //            break;
        //    //        }
        //    //    }
        //    //}

        //    container = list ?? new ConcurrentHashSet<Guid>();
        //    return found;
        //}

        //private bool CanResolve<T>(Type type,
        //                           out ISet<Guid> containerIds,
        //                           params T[] containers)
        //    where T : IDIContainer
        //{
        //    var cl = containerIds = new ConcurrentHashSet<Guid>();

        //    foreach (var container in containers)
        //    {
        //        if (((IDINinjectContainer) container).Kernel.CanResolve(type) ||
        //            CanResolve(type, out var l, ((IDIParentContainer) container).ChildContainers))
        //        {
        //            if (AddCache(type, container.Id)) cl.Add(container.Id);
        //        }
        //    }

        //    containerIds = cl;
        //    return containerIds.Any();
        //}

        //private bool CheckCache(Type T,
        //                        ref ISet<Guid> containers)
        //{
        //    if (!m_childTypeCache.ContainsKey(T)) return false;

        //    containers = m_childTypeCache[T];
        //    return true;
        //}

        //private bool AddCache(Type T,
        //                      Guid container)
        //{
        //    if (!m_childTypeCache.ContainsKey(T)) return m_childTypeCache.TryAdd(T, new ConcurrentHashSet<Guid>(container));

        //    return m_childTypeCache[T]
        //       .Add(container);
        //}

        private bool CheckParameters(Type T,
                                     out IParameter[] ninjectParams,
                                     params object[] parameters)
        {
            ninjectParams = Array.Empty<ConstructorArgument>();

            if (parameters.Length == 0) return false;

            if (parameters is Parameter[])
            {
                ninjectParams = parameters.Cast<ConstructorArgument>()
                                          .ToArray();
                return true;
            }

            //var cache = new ConcurrentDictionary<Guid, HashSet<string[]>>();
            var bindings = m_kernel.GetBindings(T);
            //.Where(b => b.Service == T);
            var names = Array.Empty<string>();
            if (m_constructorArgumentCache.ContainsKey(T.GUID))
            {
                names = m_constructorArgumentCache[T.GUID]
                   .FirstOrDefault(n => n.Length == parameters.Length);
            }
            else
                m_constructorArgumentCache.TryAdd(T.GUID, new HashSet<string[]>());

            var type = T;
            if (names.Length == 0)
            {
                if (T.IsInterface)
                {
                    var req = m_kernel.CreateRequest(T,
                                                     metadata => true,
                                                     new IParameter[0],
                                                     true,
                                                     false);
                    var cache = m_kernel.Components.Get<ICache>();
                    var planner = m_kernel.Components.Get<IPlanner>();
                    var pipeline = m_kernel.Components.Get<IPipeline>();
                    var exceptionFormatter = m_kernel.Components.Get<IExceptionFormatter>();
                    foreach (var binding in bindings)
                    {
                        var provider = binding?.GetProvider(new Context(m_kernel,
                                                                        req,
                                                                        binding,
                                                                        cache,
                                                                        planner,
                                                                        pipeline,
                                                                        exceptionFormatter));
                        type = provider?.Type;
                        if (ConstructorTest(type, parameters.Length, out names)) break;
                    }
                }
                else
                    ConstructorTest(type, parameters.Length, out names);
            }

            var ps = new ConcurrentList<ConstructorArgument>();
            for (var i = 0; i < names.Length; i++) ps.Add(new ConstructorArgument(names[i], parameters[i]));

            m_constructorArgumentCache[T.GUID]
               .Add(names);
            //var np = parameters?.Select(p =>
            //                                new ConstructorArgument("p", p));
            ninjectParams = ps.ToArray();
            return ninjectParams.Length > 0;
        }

        private IDictionary<Type, ISet<Type>> GetBindingConcreteType(Type T)
        {
            var result = new ConcurrentDictionary<Type, ISet<Type>>();
            var bindings = m_kernel.GetBindings(T);
            var req = m_kernel.CreateRequest(T,
                                             metadata => true,
                                             new IParameter[0],
                                             true,
                                             false);
            var cache = m_kernel.Components.Get<ICache>();
            var planner = m_kernel.Components.Get<IPlanner>();
            var pipeline = m_kernel.Components.Get<IPipeline>();
            var exFormatter = m_kernel.Components.Get<IExceptionFormatter>();
            foreach (var binding in bindings)
            {
                var provider = binding?.GetProvider(new Context(m_kernel,
                                                                req,
                                                                binding,
                                                                cache,
                                                                planner,
                                                                pipeline,
                                                                exFormatter));

                if (result.ContainsKey(T))
                {
                    result[T]
                       .Add(provider?.Type);
                }
                else
                    result.TryAdd(T, new ConcurrentHashSet<Type>(provider?.Type));

                // = provider?.Type;
                //if (ConstructorTest(type, parameters.Length, out names)) break;
            }

            return result;
        }

        private bool ConstructorTest(Type T,
                                     int parametersLength,
                                     out string[] parameterNames)
        {
            var cs = T.GetConstructors(BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
            var match = cs.Where(c => c.GetParameters()
                                       .Length ==
                                      parametersLength)
                          .Select(c => c.GetParameters()
                                        .Select(p => p.Name));


            parameterNames = Array.Empty<string>();
            if (match.Count() > 0)
            {
                parameterNames = match?.First()
                                       .ToArray() ??
                                 Array.Empty<string>();
            }

            return parameterNames.Length > 0;
        }

        private bool RegisterCore<T>(T? value,
                                     bool asSingleton = true,
                                     string key = null,
                                     bool overrideExisting = false)
        {
            return RegisterCore(typeof(T),
                                value,
                                asSingleton,
                                key,
                                overrideExisting);
        }

        private bool RegisterCore(Type T,
                                  object? value = null,
                                  bool asSingleton = true,
                                  string key = null,
                                  bool overrideExisting = false)
        {
            try
            {
                var ti = T.GetInterfaces()
                          .ToHashSet();
                //var it2 = ti.Select(i => i.GetTopLevelInterfaces()).SelectMany(i => i);
                //ti.AddRange(it2);
                //ti.Add(T);

                if (CheckBindings(T)) return true;

                var ib = m_kernel.Bind(ti.ToArray())
                                 .To(T);
                if (asSingleton) ib.InSingletonScope();
                if (!string.IsNullOrWhiteSpace(key)) ib.Named(key);

                var b = overrideExisting ? m_kernel.Rebind(T) : m_kernel.Bind(T);
                var fb = value is null ? b.ToSelf() : b.ToConstant(value);
                if (asSingleton) fb.InSingletonScope();
                if (!string.IsNullOrWhiteSpace(key)) fb.Named(key);

                //var ifaces = T.GetTopLevelInterfaces();
                //foreach (var i in ifaces)
                //{
                //    var b2 = overrideExisting ? m_kernel.Rebind(i) : m_kernel.Bind(i);
                //    var fb2 = value is null ? b2.To(T) : b2.ToMethod(c => c.Kernel.Get(T));
                //    if (asSingleton) fb2.InSingletonScope();
                //    if (!string.IsNullOrWhiteSpace(key)) fb2.Named(key);
                //}

                //m_kernel.Bind(x => x.FromThisAssembly()
                //                    .IncludingNonPublicTypes()
                //                    .Select(t => t == T)
                //                    .BindDefaultInterfaces()
                //.Configure((s) =>
                //           {
                //               if (asSingleton) s.InSingletonScope();
                //               if (!string.IsNullOrWhiteSpace(key)) s.Named(key);
                //           })
                //);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckBindings(Type T)
        {
            if (T.IsInterface) return false;

            var result = GetBindingConcreteType(T);
            return result.Values.Count > 0;
        }

        private bool RegisterCore<T, C>(T? value,
                                        bool asSingleton = true,
                                        string key = null,
                                        bool overrideExisting = false)
            where C : T
        {
            return RegisterCore(typeof(T),
                                typeof(C),
                                value,
                                asSingleton,
                                key,
                                overrideExisting);
        }

        private bool RegisterCore(Type T,
                                  Type C,
                                  object? value = null,
                                  bool asSingleton = true,
                                  string key = null,
                                  bool overrideExisting = false)
        {
            try
            {
                var ti = C.GetInterfaces()
                          .ToHashSet();
                ////ti.Add(T);
                //ti.Add(C);
                ti.Remove(T);
                m_kernel.Bind(ti.ToArray())
                        .To(C);
                //var b = overrideExisting ? m_kernel.Rebind(ti.ToArray()) : m_kernel.Bind(ti.ToArray());
                var b = overrideExisting ? m_kernel.Rebind(T, C) : m_kernel.Bind(T, C);
                var fb = value is null ? b.To(C) : b.ToConstant(value);

                if (asSingleton) fb.InSingletonScope();
                if (!string.IsNullOrWhiteSpace(key)) fb.Named(key);

                //m_kernel.Bind(x =>
                //              {
                //                  x.FromThisAssembly()
                //                   .IncludingNonPublicTypes()
                //                   .Select(t => t == C)
                //                   .BindAllInterfaces()
                //                   .Configure(s =>
                //                              {
                //                                  if (asSingleton) s.InSingletonScope();
                //                                  if (!string.IsNullOrWhiteSpace(key)) s.Named(key);
                //                              });
                //              });

                return true;
            }
            catch
            {
                return false;
            }
        }

        //protected bool TraverseContainers(Type type,
        //                                  out object result,
        //                                  params IParameter[] parameters)
        //{
        //    //result = null;

        //    result = m_kernel.Get(type, parameters);
        //    //if (result is not null) return true;
        //    //if (!ChildTypeCheck(type, out var container)) return false;
        //    //foreach (var id in container)
        //    //{
        //    //    ((IDINinjectContainer) m_childContainers[id]).Kernel.Get(type);
        //    //    result = m_childContainers[id]
        //    //       .Resolve(type, parameters: parameters);

        //    //    if (result is not null) break;
        //    //}

        //    return result is not null;
        //}

        //protected bool TraverseContainersAll(Type type,
        //                                     out ISet<object> result)
        //{
        //    //result = null;
        //    result = new ConcurrentHashSet<object>();
        //    //result = m_kernel.GetAll(type)
        //    //                        .ToList();
        //    if (!ChildTypeCheck(type, out var container))
        //    {
        //        result.AddRange(m_kernel.GetAll(type)
        //                                .ToList());
        //        return result.Count > 0;
        //    }

        //    //var count = result.Count;
        //    foreach (var id in container)
        //    {
        //        var currentContainer = m_childContainers[id];

        //        result.AddRange(RecursiveGetAll(type, currentContainer));

        //        //result.AddRange(m_childContainers[id]
        //        //                   .ResolveAll(type));

        //        //if (result.Count > count) break;
        //    }

        //    //if (result.Count == 0)
        //    result.AddRange(m_kernel.GetAll(type)
        //                            .ToList());

        //    return result.Count > 0;
        //}

        //private ISet<object> RecursiveGetAll(Type T,
        //                                     IDIContainer container)
        //{
        //    var result = new ConcurrentHashSet<object>();
        //    var children = ((IDIParentContainer) container).ChildContainers;
        //    //if (children?.Length > 0)
        //    //{
        //    foreach (var cc in children)
        //        //var grandChildren = ((IDIParentContainer) cc).ChildContainers;
        //        //((IDINinjectContainer)cc).Kernel.GetAll(type)
        //        result.AddRange(RecursiveGetAll(T, cc));
        //    //}
        //    //else
        //    //((IDINinjectContainer)m_childContainers[id]).Kernel.GetAll(type)
        //    result.AddRange(((IDINinjectContainer) container).Kernel.GetAll(T));

        //    return result;
        //}

        private IEnumerable<KeyValuePair<Type, IList<IBinding>>> GetAllBindings()
        {
            var baseKernel = (KernelBase) m_kernel;

            // _commandCollection is an instance, private member
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;

            // Retrieve a FieldInfo instance corresponding to the field
            var field = typeof(KernelBase).GetField("bindings", flags);

            var bindingsMap = (Multimap<Type, IBinding>) field.GetValue(baseKernel);

            bindingsMap.SelectMany(x => x.Value);

            return bindingsMap;
        }
        #endregion

        //private readonly IDIParentContainer m_parent;

        //
        //private readonly IDIRootContainer m_root;
    }
}