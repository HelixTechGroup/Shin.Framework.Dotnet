using System.Collections.Generic;

namespace Shin
{
    public interface IParent<out T>
    {
        IReadOnlyCollection<T> Children { get; }
    }
}