namespace Shin.Framework
{
    public interface IId<out T>
    {
        #region Properties
        T Id { get; }
        #endregion
    }
}