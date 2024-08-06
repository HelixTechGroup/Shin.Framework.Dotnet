using System;
using System.Collections.Generic;

namespace Shin.IoC.DependencyInjection.Runtime.Activation
{
    internal interface IDependencyGraph
    {
        IReadOnlyCollection<IDependencyNode> Nodes { get; }

        void Build(Type type);

        void Build<T>();

        internal interface IDependencyNode
        {
            Guid TypeId { get; }
            Guid ContainerId { get; }
            IReadOnlyCollection<IDependencyNode> DependsOn { get; }

            //IReadOnlyCollection<IDependencyNode> Depend { get; }
        }
    }
}