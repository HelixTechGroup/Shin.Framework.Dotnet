using System;

namespace Shin.Framework.IoC.DependencyInjection
{
    public interface IDIParentContainer : IDIContainer
    {
        #region Properties
        IDIChildContainer[] ChildContainers { get; }
        #endregion

        #region Methods
        bool IsTypeRegisteredByChild<T>();

        bool IsTypeRegisteredByChild(Type T);

        IDIChildContainer CreateChildContainer();
        #endregion
    }
}