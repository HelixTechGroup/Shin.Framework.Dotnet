using System.Collections.Generic;

using Shin.IoC.DependencyInjection.Runtime.Activation.Injectors;

namespace Shin.IoC.DependencyInjection.Runtime.Activation
{
    internal interface ITypeResolverBuilder
    {
        #region Methods
        ITypeInstance Construct(params object[] arguments);
        #endregion
    }
}