using System;
using System.Linq;
using Shin.IoC.DependencyInjection.Ninject;
using Shinject.Activation;
using Shinject.Activation.Providers;
using Shinject.Planning;
using Shinject.Selection.Heuristics;

using Shin.IoC.DependencyInjection.Shinject.Extensions;

namespace Shin.IoC.DependencyInjection.Shinject.Activation.Providers
{
    public class RootedProvider : IRootedProvider
    {
        private Type m_type;
        protected Guid m_kernelId;

        public Guid KernerId
        {
            get { return m_kernelId; }
        }

        public RootedProvider(Type type,
                              Guid kernelId)
        {
            m_type = type;
            m_kernelId = kernelId;
        }

        /// <inheritdoc />
        public object Create(IContext context)
        {
            if (context.Kernel is not IParentKernel parentKernel) return null;

            if (context.Request is IRootedRequest rootedRequest &&
                (!rootedRequest.Strategy.HasFlag(RootedResolutionStrategy.SelfOnly) &&
                 !rootedRequest.Strategy.HasFlag(RootedResolutionStrategy.NoChildren)))
            {
                var c = parentKernel.Children.Single(rr => ((IKernelWithId) rr).Id == m_kernelId);
                var r = context.Request.ToRootedRequest(parentKernel, RootedResolutionStrategy.NoParent);
                return c?.ResolveSingle(r);
            }

            return null;
        }

        /// <inheritdoc />
        public Type Type
        {
            get { return m_type; }
        }

        public static Func<IContext, IProvider> GetCreationCallback(Type prototype, Guid kernelId)
        {
            return ctx => new RootedProvider(prototype, kernelId);
        }
    }
}