using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Shin.Collections.Concurrent;
using Shin.Extensions;
using Shin.IoC.DependencyInjection;
using Shin.IoC.DependencyInjection.Runtime.Activation;
using Shin.IoC.DependencyInjection.Runtime.Activation.Collections;
using Shin.Threading;

namespace Shin.IoC.DependencyInjection.Runtime.Caching
{
    internal sealed class TypeInterfaceCache : IInterfaceMappingCache
    {
        private readonly Guid m_id;
        private static readonly object m_lock = new object();
        //private readonly ConcurrentList<IInterfaceMapping> m_mappings;
        private readonly ConcurrentDictionary<Guid, ConcurrentHashSet<Guid>> m_interfaceDictionary;
        private static readonly ReaderWriterLockSlim m_interfaceDictionaryLockSlim = new(LockRecursionPolicy.SupportsRecursion);
        private readonly IDIContainer m_container;

        /// <inheritdoc />
        public Guid Id
        {
            get { return m_id; }
        }

        public TypeInterfaceCache(IDIContainer container)
        {
            m_id = Guid.NewGuid();
            m_interfaceDictionary = new ConcurrentDictionary<Guid, ConcurrentHashSet<Guid>>();
            m_container = container;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Type> Interfaces
        {
            get { return m_interfaceDictionary.Keys.ToType()?.ToArray(); }
        }

        public bool IsRegistered(Type iface,
                          out IReadOnlyCollection<Guid> interfaces)
        {
            var typeId = iface?.GUID; //ResolveTypeId(iface);
            if (!iface.IsInterface)
            {
                interfaces = new ConcurrentList<Guid>(iface.GUID);
                return false;
            }

            m_interfaceDictionaryLockSlim.TryEnter();
            //lock(m_lock)
            {
                //try
                //{
                var cached = m_interfaceDictionary.ContainsKey(iface.GUID);

                if (!cached)
                {
                    interfaces = new List<Guid>();
                    return false;
                }

                //concrete = new ConcurrentHashSet<Guid>(iface.GUID);
                interfaces = m_interfaceDictionary[iface.GUID];
                //if (typeIds is not null)
                {
                    //concrete = new ConcurrentHashSet<Guid>(typeIds);
                    return true;
                }


                //}
                //finally
                //{
                //    m_interfaceDictionaryLockSlim.TryExit();
                //}
            }

            return false;
        }

        //private bool Check(Type iface,
        //                            out IEnumerable<Guid> concrete)
        //{
        //    Throw.IfNull(iface)
        //         .ArgumentNullException(nameof(iface));

        //    concrete = null;


        //    return RegisterInterface(iface) != null;

        //    //result = ResolveCore(match, key, strategy, parameters);
        //}

        public IReadOnlyCollection<Guid> Register(Type iface)
        {
            var concrete = new ConcurrentHashSet<Guid>();
            if (iface.IsInterface && Check(iface,
                                           out var c,
                                           m_typeDictionary.Values.ToArray()))
            {
                concrete.AddRange(c);
                m_interfaceDictionaryLockSlim.TryEnter(SynchronizationAccess.Write);
                m_interfaceDictionary.TryAdd(iface.GUID, concrete);
                m_interfaceDictionaryLockSlim.TryExit(SynchronizationAccess.Write);
            }

            return concrete;
        }

        private bool Check(Type iface,
                          out IReadOnlyCollection<Guid> interfaces)
        {
            var typeId = iface.GUID;
            var tmp = new ConcurrentHashSet<Guid>();
            interfaces = null;

            if (tmp.Count == 0)
                return false;

            interfaces = tmp;
            return true;
        }

        public IInterfaceMapping Map(Type iface, Type concrete)
        {
            Throw.If(!iface.IsInterface)
                 .InvalidOperationException();

            Throw.If(concrete.IsInterface)
                 .InvalidOperationException();

            var typeId = iface.GUID;
            var cTypeId = concrete.GUID;

            if (!m_interfaceDictionary.TryAdd(typeId, new ConcurrentHashSet<Guid>(cTypeId)))
                m_interfaceDictionary[typeId].Add(cTypeId);

            //if (iface != concrete)
            {
                foreach (var i in iface.GetInterfaces()
                                       .Select(t => t.GUID))
                {
                    if (m_interfaceDictionary.TryAdd(i, new ConcurrentHashSet<Guid>(cTypeId)))
                        continue;
                    m_interfaceDictionary[i]
                       .Add(cTypeId);
                }
            }

            foreach (var i in concrete.GetInterfaces()
                               .Select(t => t.GUID))
            {
                if (m_interfaceDictionary.TryAdd(i, new ConcurrentHashSet<Guid>(cTypeId)))
                    continue;
                var tmp = m_interfaceDictionary[i];
                m_interfaceDictionary.TryUpdate(i, new ConcurrentHashSet<Guid>(cTypeId), tmp);
            }
        }

        public IReadOnlyCollection<Guid> Unregister(Type iface)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IReadOnlyCollection<IInterfaceMapping> IInterfaceResolverMapper.Map(params ITypeResolver[] resolvers)
        {
            //var tmp = new ConcurrentHashSet<Type>();
            foreach (var i in m_interfaceDictionary)
            {
                Map(i,
                                   resolvers.Where(r => r.CheckInterface(i))
                                            .Select(r => r.Type)
                                            .ToArray());
            }
            //{
            //foreach (var resolver in resolvers.Where(r => r..ch))
            //{
            //    //var rt = resolver.Type.GUID;
            //    if (resolver.CheckInterface(i))
            //        tmp.Add(resolver.Type);
            //}

            //    m_bindingCache.Bind(i, tmp.ToArray());
            //}
        }
    }
}