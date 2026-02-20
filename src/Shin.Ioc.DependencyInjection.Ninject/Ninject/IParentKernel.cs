using System;
using System.Collections.Generic;

using Shinject;
using Shinject.Syntax;

using Shin.IoC.DependencyInjection.Shinject;

namespace Shin.IoC.DependencyInjection.Ninject
{
    public interface IParentKernel : IKernelWithId
    {
        IReadOnlyCollection<IResolutionRoot> Children { get; }
    }
}