namespace Shin.Framework.Threading
{
    public interface ISynchronize : IDispose
    {
#region Methods
        void Enter();

        void Exit();

        bool TryEnter();

        bool TryExit();
#endregion
    }

    public interface ISynchronizeObject<TObject> : ISynchronize
    {
#region Methods
        TObject Enter();

        void Exit();

        bool TryEnter(out TObject obj);

        bool TryExit();
        #endregion
    }

    public interface ISynchronize<TContext> : ISynchronize
        where TContext : ISynchronizeContext
    {
#region Methods
        TContext Create();

        bool TryCreate(out TContext context);
#endregion
    }
}