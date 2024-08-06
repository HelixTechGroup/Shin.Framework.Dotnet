using System;

using Shin.IoC.DependencyInjection;

namespace Shin.IoC.DependencyInjection.Runtime.Activation
{
    internal interface ICachedResolver : ITypeResolver
    {
        Guid Id { get; }

        Guid ContainerId { get; }

        IDIRootContainer RootContainer { get; }
    }
}