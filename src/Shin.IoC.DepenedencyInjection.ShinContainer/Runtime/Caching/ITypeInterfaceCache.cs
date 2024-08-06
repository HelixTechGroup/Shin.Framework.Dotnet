#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;

using Shin.Collections.Concurrent;
using Shin.IoC.DependencyInjection.Runtime.Activation;
#endregion

namespace Shin.IoC.DependencyInjection.Runtime.Caching
{
    internal interface IInterfaceMappingCache : IInterfaceResolverMapper,
                                                IInterfaceTypeMapper,
        /*IDictionary<Guid, ConcurrentHashSet<Guid>>,*/
        IId<Guid>
    {
        IReadOnlyDictionary<Guid, ConcurrentHashSet<Guid>> Mappings { get; }

        //IReadOnlyCollection<Type> Interfaces { get; }

        //bool Check

        bool IsRegistered(Type iface,
                   out IReadOnlyCollection<Guid> interfaces);

        bool IsMapped(Type iface, Type concrete);

        IReadOnlyCollection<Guid> Register(Type iface);

        IReadOnlyCollection<Guid> Unregister(Type iface);



        //IInterfaceMapping Unbind(Type iface,
        //                         Type concrete);
    }

    public interface IInterfaceMapping
    {
        Guid Interface { get; }

        IReadOnlyCollection<Guid> Concrete { get; }
    }

    public struct InterfaceMapping : IInterfaceMapping
    {
        private Guid m_interface;
        private ConcurrentHashSet<Guid> m_concrete;

        /// <inheritdoc />
        public Guid Interface
        {
            get { return m_interface; }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Guid> Concrete
        {
            get { return m_concrete; }
        }
    }

    internal interface IInterfaceResolverMapper
    {
        IInterfaceMapping Map(ITypeResolver resolver);

        IReadOnlyCollection<IInterfaceMapping> Map(params ITypeResolver[] resolvers);
    }

    internal interface IInterfaceTypeMapper
    {
        IInterfaceMapping Map(Type concrete);

        IReadOnlyCollection<IInterfaceMapping> Map(params Type[] concrete);
    }
}