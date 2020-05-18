using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework
{
    public interface IId<out T>
    {
        T Id { get; }
    }
}
