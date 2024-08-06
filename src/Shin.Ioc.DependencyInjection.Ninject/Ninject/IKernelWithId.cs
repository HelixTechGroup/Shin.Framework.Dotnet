using System;

using Ninject;

namespace Shin.IoC.DependencyInjection.Ninject
{
    public interface IKernelWithId : IKernel
    {
        Guid Id { get; }
    }
}