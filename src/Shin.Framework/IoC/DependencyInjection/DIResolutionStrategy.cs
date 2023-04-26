using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.IoC.DependencyInjection
{
    public enum Intensity : byte
    {
        None = 0,
        OneThird = 1,
        TwoThirds = 2,
        Max = 3
    }

    [Flags]
    public enum DIResolutionStrategy : long
    {
        Default = ChildrenFirst | ParentLast | IncludeSelf,
        Reverse = ParentFirst | ChildrenLast | IncludeSelf,
        ChildrenOnly = ChildrenFirst | ChildrenLast,
        ChildrenFirst = Intensity.OneThird << 2,
        ChildrenLast = Intensity.OneThird << 4,
        ParentOnly = ParentFirst | ParentLast,
        ParentFirst = Intensity.TwoThirds << 2,
        ParentLast = Intensity.TwoThirds << 4,
        SelfOnly = (IncludeSelf << 12),
        NoParent = IncludeSelf | ChildrenFirst,
        NoChildren = IncludeSelf | ParentLast,
        IncludeSelf = Intensity.Max 
    }
}
