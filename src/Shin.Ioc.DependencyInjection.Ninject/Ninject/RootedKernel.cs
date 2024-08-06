#region Usings
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Ninject;
using Ninject.Activation;
using Ninject.Activation.Caching;
using Ninject.Modules;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;
using Ninject.Selection.Heuristics;
using Ninject.Syntax;

using Shin.Collections.Concurrent;
using Shin.IoC.DependencyInjection.Ninject.Activation;
using Shin.IoC.DependencyInjection.Ninject.Activation.Caching;
using Shin.IoC.DependencyInjection.Ninject.Activation.Providers;
using Shin.IoC.DependencyInjection.Ninject.Extensions;
using Shin.IoC.DependencyInjection.Ninject.Planning.Bindings;
using Shin.IoC.DependencyInjection.Ninject.Selection.Heuristics;
#endregion

namespace Shin.IoC.DependencyInjection.Ninject
{
    /// <summary>
    ///     This is a kernel with a parent kernel. Any binding that can not be resolved by this kernel is forwarded to the
    ///     parent.
    /// </summary>
    public class RootedKernel : RootKernel,
                                IRootedKernel
    {
        #region Members
        //private readonly ConcurrentDictionary<Guid, IChildKernel> m_childContainers;

        /// <summary>
        ///     The parent kernel.
        /// </summary>
        private readonly IParentKernel m_parent;

        private readonly IRootKernel m_root;

        //private Guid m_id;
        #endregion

        #region Properties
        ///// <inheritdoc />
        //public IReadOnlyCollection<IResolutionRoot> Children
        //{
        //    get { return m_childContainers.Values.ToArray(); }
        //}

        ///// <inheritdoc />
        //public Guid Id
        //{
        //    get { return m_id; }
        //}

        /// <summary>
        ///     Gets the parent resolution root.
        /// </summary>
        /// <value>The parent  resolution root.</value>
        public IParentKernel Parent
        {
            get { return m_parent; }
        }

        /// <inheritdoc />
        public IRootKernel Root
        {
            get { return m_root; }
        }
        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="RootedKernel" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="modules">The modules.</param>
        public RootedKernel(IRootKernel parent)
        {
            m_parent = parent;
            m_root = (parent as IRootedKernel)?.Root ?? parent;
        }

        #region Methods
        /// <summary>
        ///     Determines whether the specified request can be resolved.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///     <c>True</c> if the request can be resolved; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanResolve(IRequest request) { return CanResolve(request, false); }

        /// <summary>
        ///     Determines whether the specified request can be resolved.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ignoreImplicitBindings">if set to <c>true</c> implicit bindings are ignored.</param>
        /// <returns>
        ///     <c>True</c> if the request can be resolved; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanResolve(IRequest request,
                                        bool ignoreImplicitBindings)
        {
            if (request is not IRootedRequest rootedRequest)
            {
                return base.CanResolve(request, ignoreImplicitBindings) ||
                       m_parent.CanResolve(request, ignoreImplicitBindings) ||
                       m_childContainers.Any(resolutionRoot => resolutionRoot.Value.CanResolve(request, ignoreImplicitBindings));
            }

            var result = false;
            var strategy = rootedRequest.Strategy;
            var selfOnly = strategy.HasFlag(RootedResolutionStrategy.SelfOnly);

            if (!selfOnly)
            {

                if (strategy.HasFlag(RootedResolutionStrategy.ChildrenOnly) ||
                    strategy.HasFlag(RootedResolutionStrategy.ChildrenFirst))
                    result = CanResolveChildren(rootedRequest, ignoreImplicitBindings, out _);

                if (!result &&
                    (strategy.HasFlag(RootedResolutionStrategy.ParentFirst) || strategy.HasFlag(RootedResolutionStrategy.ParentOnly)))
                    result = CanResolveParent(rootedRequest);
            }

            if (!result &&
                (strategy.HasFlag(RootedResolutionStrategy.IncludeSelf) || selfOnly))
                result = base.CanResolve(request, ignoreImplicitBindings);

            if (!result &&
                !selfOnly)
            {
                if (strategy.HasFlag(RootedResolutionStrategy.ChildrenLast)) result = CanResolveChildren(rootedRequest, ignoreImplicitBindings, out _);

                if (!result &&
                    strategy.HasFlag(RootedResolutionStrategy.ParentLast))
                    result = CanResolveParent(rootedRequest);
            }

            return result;
        }

        //protected IRootedTypeMap GetRootedTypeMap(IRootedRequest rootedRequest)
        //{

        //}

        //protected T ResolveTypeMap<T>(IRootedTypeMap map)
        //{

        //}

        /// <summary>
        ///     Resolves instances for the specified request. The instances are not actually resolved
        ///     until a consumer iterates over the enumerator.
        /// </summary>
        /// <param name="request">The request to resolve.</param>
        /// <returns>
        ///     An enumerator of instances that match the request.
        /// </returns>
        public override IEnumerable<object> Resolve(IRequest request)
        {
            if (request is not IRootedRequest rootedRequest) rootedRequest = request.ToRootedRequest(this);

            var result = new ConcurrentList<object>();
            var strategy = rootedRequest.Strategy;
            var selfOnly = strategy.HasFlag(RootedResolutionStrategy.SelfOnly);

            try
            {
                if (!selfOnly)
                {
                    if (strategy.HasFlag(RootedResolutionStrategy.ChildrenOnly) ||
                        strategy.HasFlag(RootedResolutionStrategy.ChildrenFirst))
                        result.AddRange(ResolveChildren(rootedRequest));

                    if (strategy.HasFlag(RootedResolutionStrategy.ParentFirst) ||
                        strategy.HasFlag(RootedResolutionStrategy.ParentOnly))
                        result.AddRange(ResolveParent(rootedRequest));
                }

                try
                {
                    if (strategy.HasFlag(RootedResolutionStrategy.IncludeSelf) ||
                        (selfOnly && base.CanResolve(rootedRequest)))
                        result.AddRange(base.Resolve(rootedRequest));
                }
                catch (ActivationException)
                {

                }

                if (!selfOnly)
                {
                    if (strategy.HasFlag(RootedResolutionStrategy.ChildrenLast)) result.AddRange(ResolveChildren(rootedRequest));

                    if (strategy.HasFlag(RootedResolutionStrategy.ParentLast)) result.AddRange(ResolveParent(rootedRequest));
                }
            }
            catch (ActivationException)
            {
                try
                {
                    return m_parent.Resolve(request);
                }
                catch (ActivationException) { }

                throw;
            }

            return result;
        }

        /// <inheritdoc />
        public override IRequest CreateRequest(Type service,
                                               Func<IBindingMetadata, bool> constraint,
                                               IReadOnlyList<IParameter> parameters,
                                               bool isOptional,
                                               bool isUnique)
        {
            return this.CreateRootedRequest(service,
                                            RootedResolutionStrategy.Default,
                                            constraint,
                                            parameters,
                                            isOptional,
                                            isUnique);
        }

        /// <inheritdoc />
        public new object ResolveSingle(IRequest request)
        {
            if (request is not IRootedRequest rootedRequest) rootedRequest = request.ToRootedRequest(this);

            object result = null;
            var strategy = rootedRequest.Strategy;
            var selfOnly = strategy.HasFlag(RootedResolutionStrategy.SelfOnly);
            try
            {
                if (!selfOnly)
                {
                    if (strategy.HasFlag(RootedResolutionStrategy.ChildrenOnly) ||
                        strategy.HasFlag(RootedResolutionStrategy.ChildrenFirst))
                        result = ResolveSingleChildren(rootedRequest);

                    if (result is null && (strategy.HasFlag(RootedResolutionStrategy.ParentFirst) || strategy.HasFlag(RootedResolutionStrategy.ParentOnly)))
                        result = ResolveParent(rootedRequest);
                }

                try
                {
                    if (result is null && strategy.HasFlag(RootedResolutionStrategy.IncludeSelf) ||
                        (selfOnly && base.CanResolve(rootedRequest)))
                        result = base.ResolveSingle(rootedRequest);
                }
                catch (ActivationException)
                {
                    result = null;
                }

                if (!selfOnly)
                {
                    if (result is null &&
                        strategy.HasFlag(RootedResolutionStrategy.ChildrenLast))
                        result = ResolveSingleChildren(rootedRequest);

                    if (result is null &&
                        strategy.HasFlag(RootedResolutionStrategy.ParentLast))
                        result = ResolveSingleParent(rootedRequest);
                }
            }
            catch (ActivationException)
            {
                try
                {
                    return m_parent?.ResolveSingle(request);
                }
                catch (ActivationException) { }

                throw;
            }

            return result;
        }

        /// <inheritdoc />
        protected override void AddComponents()
        {
            base.AddComponents();

            Components.RemoveAll<IActivationCache>();
            Components.Add<IActivationCache, RootedActivationCache>();

            Components.RemoveAll<IConstructorScorer>();
            Components.Add<IConstructorScorer, RootedConstructorScorer>();

            Components.Add<IBindingResolver, RootedBindingResolver>();

            Components.Add<IMissingBindingResolver, RootedMissingBindingResolver>();
        }

        /// <inheritdoc />
        protected override bool SatifiesRequest(IRequest request,
                                                IBinding binding)
        {
            return base.SatifiesRequest(request, binding);
        }

        protected object ResolveSingleChildren(IRootedRequest rootedRequest)
        {
            if (!CanResolveChildren(rootedRequest, false, out var ids)) return null;

            return m_childContainers[ids.First()]
               .ResolveSingle(rootedRequest);
        }

        protected IEnumerable<object> ResolveChildren(IRootedRequest rootedRequest)
        {
            var result = new ConcurrentList<object>();
            if (!CanResolveChildren(rootedRequest, false, out var ids)) return result;

            foreach (var id in ids)
            {
                result.AddRange(m_childContainers[id]
                                   .Resolve(rootedRequest));
            }

            return result;
        }

        protected void AddRootedBinding(Type type,
                                        Guid kernelId)
        {
            if (!m_childContainers.ContainsKey(kernelId)) throw new ActivationException();

            //var b = new BindingConfiguration();
            //b.ProviderCallback = context => new RootedProvider(type, kernelId);
            var b = new Binding(type)
            {
                ProviderCallback = RootedProvider.GetCreationCallback(type, kernelId)
            };
            b.Metadata.Set("kernelId", kernelId);
            AddBinding(b);
        }

        protected object ResolveSingleParent(IRootedRequest rootedRequest)
        {
            var r = rootedRequest;
            if (rootedRequest.RequestingKernelId != m_id)
            {
                r = this.CreateRootedRequest(rootedRequest.Service,
                                             RootedResolutionStrategy.Default,
                                             rootedRequest.Constraint,
                                             rootedRequest.Parameters,
                                             rootedRequest.IsOptional,
                                             rootedRequest.IsUnique) as IRootedRequest;
            }

            if (CanResolveParent(r)) return m_parent.ResolveSingle(r);

            return null;
        }

        protected IEnumerable<object> ResolveParent(IRootedRequest rootedRequest)
        {
            var r = rootedRequest;
            if (rootedRequest.RequestingKernelId != m_id)
            {
                r = this.CreateRootedRequest(rootedRequest.Service,
                                             RootedResolutionStrategy.Default,
                                             rootedRequest.Constraint,
                                             rootedRequest.Parameters,
                                             rootedRequest.IsOptional,
                                             rootedRequest.IsUnique) as IRootedRequest;
            }

            if (CanResolveParent(r)) return m_parent.Resolve(r);

            return Array.Empty<object>();
        }

        /// <inheritdoc />
        //protected override IContext CreateContext(IRequest request,
        //                                          IBinding binding)
        //{
        //    if (request is not IRootedRequest)
        //        return base.CreateContext(request, binding);

        //    return new RootedContext((IKernel) this,
        //                             request,
        //                             binding,
        //                             this.Components.Get<ICache>(),
        //                             this.Components.Get<IPlanner>(),
        //                             this.Components.Get<IPipeline>(),
        //                             this.Components.Get<IExceptionFormatter>());
        //}
        private bool CanResolveParent(IRootedRequest rootedRequest)
        {
            var r = rootedRequest;
            if (rootedRequest.RequestingKernelId != m_id)
            {
                r = this.CreateRootedRequest(rootedRequest.Service,
                                             RootedResolutionStrategy.Default,
                                             rootedRequest.Constraint,
                                             rootedRequest.Parameters,
                                             rootedRequest.IsOptional,
                                             rootedRequest.IsUnique) as IRootedRequest;
            }

            return m_parent.CanResolve(r, true);
        }

        private bool CheckBindings(Type type,
                                   out IReadOnlyList<IBinding> bindings)
        {
            //bindings = GetBindings(type)
            //   .Where(b => b is IRootedBinding).ToList();
            //return bindings.Any();

            //foreach (var binding in GetBindings(rootedRequest.Service))
            //{
            var b = GetBindings(type);
            var tmp = new ConcurrentList<IBinding>();
            Array.ForEach<IBinding>(b.ToArray(),
                          binding =>
                          {
                              if (binding.Metadata.Has("kernelId")) tmp.Add(binding);
                              //var c = CreateContext(rootedRequest, binding);
                              //if (binding.GetProvider(c) is IRootedProvider rootedProvider) tmp.Add(rootedProvider.KernerId);
                          });
            //}

            bindings = tmp;
            return tmp.Any();
        }

        private bool CanResolveChildren(IRootedRequest rootedRequest,
                                        bool ignoreImplicitBindings,
                                        out IReadOnlyList<Guid> kernelId)
        {
            var tmp = new ConcurrentList<Guid>();

            if (CheckBindings(rootedRequest.Service, out var bindings))
            {
                kernelId = bindings.Select(b => b.Metadata.Get<Guid>("kernelId"))
                                   .ToList();
                return true;
            }

            var r = this.CreateRootedRequest(rootedRequest.Service,
                                             RootedResolutionStrategy.NoParent,
                                             rootedRequest.Constraint,
                                             rootedRequest.Parameters,
                                             rootedRequest.IsOptional,
                                             rootedRequest.IsUnique);
            foreach (var c in m_childContainers)
            {
                if (c.Key == rootedRequest.RequestingKernelId ||
                    !c.Value.CanResolve(r, ignoreImplicitBindings))
                    continue;

                AddRootedBinding(rootedRequest.Service, c.Key);
                tmp.Add(c.Key);
            }

            kernelId = tmp.ToArray();
            return kernelId.Any();
        }
        #endregion
    }
}