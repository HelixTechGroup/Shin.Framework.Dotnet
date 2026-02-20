using System;
using System.Collections.Generic;

using Shinject.Planning.Targets;

namespace Shin.IoC.DependencyInjection.Shinject.Selection
{
    public interface IRootedTypeMap
    {
        int Depth { get; }
        ITarget Target { get; }
        IReadOnlyCollection<IRootedTypeMapEntry> Entries { get; }
    }

    public interface IRootedTypeMapEntry
    {
        Guid KernelId { get; }
        Guid NextKernelId { get; }
    }
}