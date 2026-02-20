using Shin.IoC.DependencyInjection.Ninject;
using Shinject;
using Shinject.Activation;

using Shin.IoC.DependencyInjection.Shinject.Activation;

namespace Shin.IoC.DependencyInjection.Shinject.Extensions
{
    public static class RequestExtenstions
    {
        internal static IRootedRequest ToRootedRequest(this IRequest request, IKernelWithId kernel, RootedResolutionStrategy strategy = RootedResolutionStrategy.Default)
        {
            return new RootedRequest(request,
                                     kernel,
                                     strategy);
        }
    }
}