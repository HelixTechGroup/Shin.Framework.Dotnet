using System;

namespace Shin.Framework.IoC.DependencyInjection
{
    public interface IDIChildContainer : IDIContainer
    {
        #region Properties
        IDIParentContainer ParentContainer { get; }
        #endregion

        #region Methods
        bool IsTypeRegisteredByParent<T>();

        bool IsTypeRegisteredByParent(Type T);
        #endregion
    }
}