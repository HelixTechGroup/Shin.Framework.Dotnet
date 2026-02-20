using System;

using Shinject;

namespace Shin.IoC.DependencyInjection.Ninject
{
    public interface IKernelWithId : IKernel
    {
        Guid Id { get; }
    }
}