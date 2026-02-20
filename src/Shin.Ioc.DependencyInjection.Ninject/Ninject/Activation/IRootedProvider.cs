using System;

using Shinject.Activation;

namespace Shin.IoC.DependencyInjection.Shinject.Activation
{
    public interface IRootedProvider : IProvider
    {
        Guid KernerId { get; }
    }
}