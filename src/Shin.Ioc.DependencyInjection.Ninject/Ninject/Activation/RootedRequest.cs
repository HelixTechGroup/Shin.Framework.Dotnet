#region Usings
using System;
using System.Collections.Generic;

using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Planning.Targets;
#endregion

namespace Shin.IoC.DependencyInjection.Ninject.Activation
{
    public class RootedRequest : Request,
                                 IRootedRequest
    {
        #region Members
        private readonly RootedResolutionStrategy m_strategy;
        private Guid m_kernelId;
        #endregion

        #region Properties
        /// <inheritdoc />
        public Guid RequestingKernelId
        {
            get { return m_kernelId; }
        }

        /// <inheritdoc />
        public RootedResolutionStrategy Strategy
        {
            get { return m_strategy; }
        }
        #endregion

        internal RootedRequest(IRequest request,
                             IKernelWithId kernel,
                             RootedResolutionStrategy strategy) : this(request.Service,
                                                                       request.Constraint,
                                                                       request.Parameters,
                                                                       request.GetScope,
                                                                       request.IsOptional,
                                                                       request.IsUnique,
                                                                       kernel.Id,
                                                                       strategy)
        {

        }

        /// <inheritdoc />
        public RootedRequest(Type service,
                             Func<IBindingMetadata, bool> constraint,
                             IReadOnlyList<IParameter> parameters,
                             Func<object> scopeCallback,
                             bool isOptional,
                             bool isUnique) : this(service,
                                                   constraint,
                                                   parameters,
                                                   scopeCallback,
                                                   isOptional,
                                                   isUnique,
                                                   Guid.Empty,
                                                   RootedResolutionStrategy.Default)
        { }

        /// <inheritdoc />
        public RootedRequest(IContext parentContext,
                             Type service,
                             ITarget target,
                             Func<object> scopeCallback) : this(parentContext,
                                                                service,
                                                                target,
                                                                scopeCallback,
                                                                RootedResolutionStrategy.Default)
        { }

        public RootedRequest(Type service,
                             Func<IBindingMetadata, bool> constraint,
                             IReadOnlyList<IParameter> parameters,
                             Func<object> scopeCallback,
                             bool isOptional,
                             bool isUnique,
                             Guid requestingKernelId,
                             RootedResolutionStrategy strategy) : base(service,
                                                                       constraint,
                                                                       parameters,
                                                                       scopeCallback,
                                                                       isOptional,
                                                                       isUnique)
        {
            m_strategy = strategy;
            m_kernelId = requestingKernelId;
        }

        public RootedRequest(Type service,
                             Func<IBindingMetadata, bool> constraint,
                             IReadOnlyList<IParameter> parameters,
                             Func<object> scopeCallback,
                             bool isOptional,
                             bool isUnique,
                             RootedResolutionStrategy strategy) : base(service,
                                                                       constraint,
                                                                       parameters,
                                                                       scopeCallback,
                                                                       isOptional,
                                                                       isUnique)
        {
            m_strategy = strategy;
            m_kernelId = Guid.Empty;
        }

        public RootedRequest(IContext parentContext,
                             Type service,
                             ITarget target,
                             Func<object> scopeCallback,
                             RootedResolutionStrategy strategy) : base(parentContext, service, target, scopeCallback)
        {
            if (parentContext.Kernel is IParentKernel p)
                m_kernelId = p.Id;

            m_strategy = strategy;
        }
    }
}