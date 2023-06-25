#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using sysComponent = System.ComponentModel;
#endregion

namespace Shin.Framework.IoC.DependencyInjection
{
    public partial interface IDIContainer : IDispose
    {
        #region Properties
        Guid Id { get; }

        IEnumerable<Type> RegisteredTypes { get; }

        IDIRootContainer Root { get; }
        #endregion

        #region Methods
        void Load(params IBindings[] bindings);

        void Unload(params IBindings[] bindings);
        
        void Release();
        #endregion
    }
}