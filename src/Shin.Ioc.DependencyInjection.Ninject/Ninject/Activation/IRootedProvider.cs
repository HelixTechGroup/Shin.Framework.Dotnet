using System;

using Ninject.Activation;

namespace Shin.IoC.DependencyInjection.Ninject.Activation
{
    public interface IRootedProvider : IProvider
    {
        Guid KernerId { get; }
    }
}