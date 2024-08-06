#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ninject.Activation;
using Ninject.Components;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;

using Shin.Collections.Concurrent;
using Shin.IoC.DependencyInjection.Ninject.Activation;
using Shin.IoC.DependencyInjection.Ninject.Activation.Providers;
#endregion

namespace Shin.IoC.DependencyInjection.Ninject.Planning.Bindings
{
    public class RootedMissingBindingResolver : NinjectComponent,
                                                IMissingBindingResolver
    {
        #region Methods
        /// <inheritdoc />
        public ICollection<IBinding> Resolve([NotNull] IDictionary<Type, ICollection<IBinding>> bindings,
                                             [NotNull] IRequest request)
        {
            var service = request.Service;
            if (request is not IRootedRequest rootedRequest ||
                !bindings.TryGetValue(service, out var sb))
                return Array.Empty<IBinding>();

            var tmp = sb.Where(b => b.Metadata.Has("kernelId")).ToArray();
            if (tmp.Length > 0) return tmp;

            var kernelId = rootedRequest.RequestingKernelId;
            var b = new Binding(service)
            {
                ProviderCallback = RootedProvider.GetCreationCallback(service, kernelId)
            };
            b.Metadata.Set("kernelId", kernelId);

            return new[]
                    {
                        b
                    };

        }
        #endregion
    }
}