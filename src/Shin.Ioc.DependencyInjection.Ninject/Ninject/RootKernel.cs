using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Shinject;
using Shinject.Activation;
using Shinject.Modules;
using Shinject.Parameters;
using Shinject.Planning.Bindings;
using Shinject.Syntax;

using Shin.Collections.Concurrent;

namespace Shin.IoC.DependencyInjection.Ninject
{
    public class RootKernel : StandardKernel, IRootKernel
    {

        protected readonly ConcurrentDictionary<Guid, IChildKernel> m_childContainers = new();


        protected readonly ConcurrentDictionary<Type, ConcurrentHashSet<Guid>> m_childTypeCache = new();

        //
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