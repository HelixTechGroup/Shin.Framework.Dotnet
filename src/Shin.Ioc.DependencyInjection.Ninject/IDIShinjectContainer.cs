using System;

using Shinject;

using Shin.IoC.DependencyInjection;

namespace Shin.IoC.DependencyInjection
{
    public interface IDIShinjectContainer : IDIContainer,
                                             IDIChildContainer,
                                             IDIParentContainer,
                                             IDIRootContainer
    {
        IKernel Kernel { get; }

        //bool TypeCheckContainers(Type T,
        //                         out IDIContainer[] containers);
    }
}