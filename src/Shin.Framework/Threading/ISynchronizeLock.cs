namespace Shin.Framework.Threading
{
    public interface ISynchronizeLockReader : ISynchronizeLock
    {
#region Properties
        bool IsReadLockHeld { get; }
#endregion

#region Methods
        void EnterRead();

        void ExitRead();
#endregion
    }

    public interface ISynchronizeLockWriter : ISynchronizeLock
    {
#region Properties
        bool IsWriteLockHeld { get; }
#endregion

#region Methods
        void EnterWrite();

        void ExitWrite();
#endregion
    }

    public interface ISynchronizeLockUpgrader : ISynchronizeLock
    {
#region Methods
        bool Upgrade();

        void Downgrade();
#endregion

        //#region Properties
        //        bool IsUpgradeableLockHeld { get; }
        //#endregion
    }

    public interface ISynchronizeLock : ISynchronize
    {
#region Properties
        bool IsLocked { get; set; }
#endregion
    }
}