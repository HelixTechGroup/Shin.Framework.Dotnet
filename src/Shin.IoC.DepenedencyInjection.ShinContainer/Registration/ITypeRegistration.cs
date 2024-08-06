#region Usings
using System;

using Shin.IoC.DependencyInjection.Registration.Collections;
#endregion

namespace Shin.IoC.DependencyInjection.Registration
{
    internal interface ITypeRegistration : IId<Guid>
    {
        #region Properties
        string Key { get; }
        RegistrationScope Scope { get; }
        Guid Concrete { get; }
        Guid Interface { get; }
        #endregion
    }
}