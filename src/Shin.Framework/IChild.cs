namespace Shin
{
    public interface IChild<out T>
    {
        T Parent { get; }
    }
}