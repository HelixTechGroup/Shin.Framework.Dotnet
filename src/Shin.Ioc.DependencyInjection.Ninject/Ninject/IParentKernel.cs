using System;
using System.Collections.Generic;

using Ninject;
using Ninject.Syntax;

using Shin.IoC.DependencyInjection.Ninject;

namespace Shin.IoC.DependencyInjection.Ninject
{
    public interface IParentKernel : IKernelWithId
    {
        IReadOnlyCollection<IResolutionRoot> Children { get; }
    }
}