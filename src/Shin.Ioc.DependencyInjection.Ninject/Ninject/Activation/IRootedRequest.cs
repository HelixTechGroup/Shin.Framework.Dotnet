using System;

using Shinject.Activation;

namespace Shin.IoC.DependencyInjection.Shinject.Activation
{
    public interface IRootedRequest : IRequest
    {
        Guid RequestingKernelId { get; }

        RootedResolutionStrategy Strategy { get; }
    }
}