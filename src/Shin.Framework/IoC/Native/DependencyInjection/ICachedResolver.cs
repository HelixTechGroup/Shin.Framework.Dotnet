using System;
using Shin.Framework.IoC.DependencyInjection;

namespace Shin.Framework.IoC.Native.DependencyInjection
{
    internal interface ICachedResolver : IResolver
    {
        Guid Id { get; }

        Guid ContainerId { get; }
        
        IDIRootContainer RootContainer { get; }
    }
}