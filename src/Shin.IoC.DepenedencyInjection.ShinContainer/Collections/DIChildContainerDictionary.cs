using System;

using Shin.Collections.Concurrent;
using Shin.IoC.DependencyInjection;

namespace Shin.IoC.DependencyInjection.Collections
{
    internal class DIChildContainerDictionary : DisposableConcurrentDictionary<Guid, IDIChildContainer>
    {

    }
}