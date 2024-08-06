using System;

using Ninject.Activation;

namespace Shin.IoC.DependencyInjection.Ninject.Activation
{
    public interface IRootedRequest : IRequest
    {
        Guid RequestingKernelId { get; }

        RootedResolutionStrategy Strategy { get; }
    }
}