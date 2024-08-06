using System;
using System.Collections.Concurrent;

namespace Shin.IoC.DependencyInjection.Registration.Collections
{
    internal class TypeRegistrationDictionary : ConcurrentDictionary<Guid, ITypeRegistration>
    {

    }
}