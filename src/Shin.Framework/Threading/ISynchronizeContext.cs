namespace Shin.Framework.Threading
{
    public interface ISynchronizeContext : IDispose
    {
        SynchronizationAccess AccessLevel { get; }
        bool IsSynchronized { get; }
    }
}
