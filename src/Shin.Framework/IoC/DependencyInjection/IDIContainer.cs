#region Usings
using System;
using System.Collections.Generic;
#endregion

namespace Shin.IoC.DependencyInjection
{
    public partial interface IDIContainer : IDispose
    {
        #region Properties
        Guid Id { get; }

        IEnumerable<Type> RegisteredTypes { get; }

        IEnumerable<Type> RegisteredInterfaces { get; }
        
        IDIRootContainer Root { get; }
        #endregion

        #region Methods
        void Load(params IBindings[] bindings);

        void Unload(params IBindings[] bindings);
        
        void Release();
        #endregion
    }
}