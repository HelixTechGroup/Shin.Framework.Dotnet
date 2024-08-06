using System;

using Ninject;

using Shin.IoC.DependencyInjection;

namespace Shin.IoC.DependencyInjection
{
    public interface IDINinjectContainer : IDIContainer,
                                             IDIChildContainer,
                                             IDIParentContainer,
                                             IDIRootContainer
    {
        IKernel Kernel { get; }

        //bool TypeCheckContainers(Type T,
        //                         out IDIContainer[] containers);
    }
}