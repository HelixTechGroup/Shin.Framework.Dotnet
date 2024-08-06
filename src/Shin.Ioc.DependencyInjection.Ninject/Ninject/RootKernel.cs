using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Syntax;

using Shin.Collections.Concurrent;

namespace Shin.IoC.DependencyInjection.Ninject
{
    public class RootKernel : StandardKernel, IRootKernel
    {
        [NotNull]
        protected readonly ConcurrentDictionary<Guid, IChildKernel> m_childContainers = new();

        [NotNull]
        protected readonly ConcurrentDictionary<Type, ConcurrentHashSet<Guid>> m_childTypeCache = new();

        //[NotNull]
        protected readonly Guid m_id = Guid.NewGuid();
        //private IKernel m_kernelInstance;

        /// <inheritdoc />
        public Guid Id
        {
            get { return m_id; }
        }

        /// <inheritdoc />
        protected override void AddComponents()
        {
            base.AddComponents();
            Bind<IRootedKernel>()
               .ToMethod(context => new RootedKernel(this));
        }

        /// <inheritdoc />
        //public override IBinding[] GetBindings(Type service) { return base.GetBindings(service); }

        public IEnumerable<IBinding> GetBindings(Type service)
        {
            return base.GetBindings(service);

        }

        /// <inheritdoc />
        public override bool CanResolve(IRequest request) { return base.CanResolve(request); }

        /// <inheritdoc />
        protected override bool HandleMissingBinding(IRequest request) { return base.HandleMissingBinding(request); }

        /// <inheritdoc />
        public IReadOnlyCollection<IResolutionRoot> Children
        {
            get { return m_childContainers.Values.ToArray(); }
        }
    }
}