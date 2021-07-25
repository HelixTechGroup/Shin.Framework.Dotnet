namespace Shin.Framework.Threading
{
    public interface ISynchronize : IDispose
    {
        void Enter();

        void Exit();
    }

    public interface ISynchronize<TObject> : IDispose
    {
        TObject Enter();

        void Exit();
    }
}
