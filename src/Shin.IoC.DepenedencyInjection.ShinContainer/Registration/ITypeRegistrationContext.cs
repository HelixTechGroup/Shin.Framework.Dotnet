#region Usings
using System;
#endregion

namespace Shin.IoC.DependencyInjection.Registration
{
    internal interface ITypeRegistrationContext : IId<Guid>, IDispose
    {
        #region Properties
        bool IsSingleton { get; }
        Guid Concrete { get; }
        object Instance { get; }
        Guid Interface { get; }
        string Key { get; }
        bool Force { get; }
        ITypeRegistration Registration { get; }
        #endregion
    }
}