#region Usings
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Shin.Collections.Concurrent;
using Shin.Extensions;

namespace Shin.IoC.DependencyInjection.Runtime.Activation.Collections
{
    internal sealed class TypeResolverDictionary : DisposableConcurrentDictionary<Guid, ITypeResolver>
    {
        private ConcurrentHashSet<IDisposable> m_trackedDisposables;

        public IReadOnlyCollection<Type> RegisteredTypes
        {
            get { return Keys.ToType()?.ToArray(); }
        }

        public IReadOnlyCollection<ITypeResolver> Resolvers
        {
            get { return Values.ToArray(); }

        }

        public void Compile()
        {

        }
    }
}