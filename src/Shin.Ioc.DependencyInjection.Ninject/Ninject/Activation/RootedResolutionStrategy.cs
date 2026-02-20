using System;

namespace Shin.IoC.DependencyInjection.Shinject.Activation
{
    [Flags]
    public enum RootedResolutionStrategy : long
    {
        Default = ChildrenFirst | ParentLast | IncludeSelf,
        Reverse = ParentFirst | ChildrenLast | IncludeSelf,
        ChildrenOnly = ChildrenFirst | ChildrenLast,
        ChildrenFirst = 1 << 2,
        ChildrenLast = 1 << 4,
        ParentOnly = ParentFirst | ParentLast,
        ParentFirst = 2 << 2,
        ParentLast = 2 << 4,
        SelfOnly = (IncludeSelf << 12),
        NoParent = IncludeSelf | ChildrenFirst,
        NoChildren = IncludeSelf | ParentLast,
        IncludeSelf = 3
    }
}
