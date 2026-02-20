using Shin.IoC.DependencyInjection.Shinject;

namespace Shin.IoC.DependencyInjection.Ninject
{
    public interface IRootedKernel : IRootKernel, IChildKernel
    {
        IRootKernel Root { get; }
    }
}