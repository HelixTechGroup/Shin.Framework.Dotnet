namespace Shin.Framework.Threading
{
    public interface ISynchronizeLock : ISynchronize
    {
        ISynchronizeContext Context { get; }
    }
}
