using Ninject;
using Ninject.Activation;

using Shin.IoC.DependencyInjection.Ninject.Activation;

namespace Shin.IoC.DependencyInjection.Ninject.Extensions
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