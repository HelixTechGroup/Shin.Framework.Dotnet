using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BK.Util
{
    internal class LockDetails<Key, Value> where Key:IComparable<Key>
    {
        private int numberOfThreadsLockedThisKey;
        private Dictionary<string, ThreadDetails<Key, Value>> listOfThreadsLockingItems;
        private List<Value> listOfValuesMarkedForDeletion;
        private AutoResetEvent eventToWaitOnDeletion;
        private bool itemMarkedForDeleteAll;
        private bool itemMarkedForDelete;
        private MultimapBK<Key,Value> mainInstance;
        private object lockThis = new object();
        private int NotifyOnDeletion;

        internal bool IsItemVisibleToThisThread(string ThreadId)
        {
            bool retValue = false;
            ThreadDetails<Key,Value> result = null;
            if (listOfThreadsLockingItems.TryGetValue(ThreadId, out result))
            {
                retValue = true;
            }

            return retValue;
        }

        internal LockDetails(MultimapBK<Key, Value> MultiMapInstance)
        {
            mainInstance = MultiMapInstance;
            listOfValuesMarkedForDeletion = new List<Value>();
            listOfThreadsLockingItems = new Dictionary<string, ThreadDetails<Key, Value>>();
            eventToWaitOnDeletion = new AutoResetEvent(false);
            NotifyOnDeletion = 0;
        }

        internal void Initialize(MultimapBK<Key,Value> MultiMapInstance, Key KeyInAccess, ThreadDetails<Key,Value> ThreadRequesting)
        {
            lock (lockThis)
            {
                itemMarkedForDeleteAll = false;
                ThreadDetails<Key, Value> threadDetails = null;
                if (!listOfThreadsLockingItems.TryGetValue(ThreadRequesting.ThreadId, out threadDetails))
                {
                    numberOfThreadsLockedThisKey++;
                }
                // else this item is already locked by this Thread.  So no need to increment the count.
            }
        }
        internal void RemoveLock(Key KeyInAccess, ThreadDetails<Key, Value> ThreadRequesting)
        {
            ThreadDetails<Key, Value> threadDetails = null;
            if (listOfThreadsLockingItems.TryGetValue(ThreadRequesting.ThreadId, out threadDetails))
            {
                lock (lockThis)
                {
                    numberOfThreadsLockedThisKey--;
                    listOfThreadsLockingItems.Remove(ThreadRequesting.ThreadId);
                    if (numberOfThreadsLockedThisKey <= 0 && itemMarkedForDeleteAll)
                    {
                        mainInstance.DeleteAll(KeyInAccess);
                    }
                    else if (numberOfThreadsLockedThisKey <= 0 && itemMarkedForDelete)
                    {
                        int Counter = 0;
                        while (Counter < listOfValuesMarkedForDeletion.Count)
                        {
                            mainInstance.Delete(KeyInAccess, listOfValuesMarkedForDeletion[Counter]);
                            Counter++;
                        }

                        listOfValuesMarkedForDeletion.Clear();
                        itemMarkedForDelete = false;

                        while (NotifyOnDeletion != 0)
                        {
                            eventToWaitOnDeletion.Set();
                            NotifyOnDeletion--;
                        }
                    }
                }
            }
        }

        internal void AddLock(Key KeyInAccess, ThreadDetails<Key, Value> ThreadRequesting)
        {
            ThreadDetails<Key, Value> threadDetails = null;
            if (!listOfThreadsLockingItems.TryGetValue(ThreadRequesting.ThreadId, out threadDetails))
            {
                numberOfThreadsLockedThisKey++;
                listOfThreadsLockingItems.Add(ThreadRequesting.ThreadId, ThreadRequesting);
            }
        }

        internal bool ItemMarkedForDeleteAll
        {
            get
            {

                return itemMarkedForDeleteAll;
            }
        }

        internal bool ItemMarkedForDelete
        {
            get
            {
                return itemMarkedForDelete;
            }
        }

        internal void AddValueToBeDeleted(Key KeyItem, Value ValueToBeDeleted)
        {
            lock (lockThis)
            {
                if (numberOfThreadsLockedThisKey <= 0)
                {
                    mainInstance.Delete(KeyItem, ValueToBeDeleted);
                }
                else
                {
                    itemMarkedForDelete = true;
                    listOfValuesMarkedForDeletion.Add(ValueToBeDeleted);
                }
            }
        }

        internal void AddKeyToBeDeleted(Key KeyItem)
        {
            lock (lockThis)
            {
                if (numberOfThreadsLockedThisKey <= 0)
                {
                    mainInstance.DeleteAll(KeyItem);
                }
                else
                {
                    itemMarkedForDeleteAll = true;
                }
            }
        }

        internal AutoResetEvent NotifyOnDeletionComplete()
        {
            NotifyOnDeletion++;

            return eventToWaitOnDeletion;
        }




    }
}
