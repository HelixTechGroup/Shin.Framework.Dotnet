using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;


namespace BK.Util
{
  
    /// <summary>
    /// A Generic Dictionary collection type that can store multiple values for the same key.
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class MultimapBK<Key, Value> : IDisposable, IEnumerable<KeyValuePair<Key, ValueItem<Key, Value>>> where Key:IComparable<Key>
    {
        private Dictionary<Key, List<Value>> dictMultiMap;
        private List<Key> listForEnumerator;
        private Dictionary<string, ThreadDetails<Key, Value>> dictForThreadOrSession;
        private Dictionary<Key, LockDetails<Key, Value>> lockDetailsForKeys;
        
        private bool thisIsNotWebProject;
        private bool dispose;
        
        /// <summary>
        /// Construction of Multi map
        /// </summary>
        public MultimapBK()
        {
            try
            {
                Initialize();
            }
            catch(Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }
        }

        /// <summary>
        /// Gets the Enumerator
        /// </summary>
        /// <returns></returns>
        public MultimapEnum GetEnumerator()
        {
            return new MultimapEnum(this);
        }

        /// <summary>
        /// Construction copying from another Multi map
        /// </summary>
        /// <param name="DictToCopy"></param>
        public MultimapBK(ref MultimapBK<Key, Value> DictToCopy)
        {
            try
            {
                Initialize();

                if (DictToCopy != null)
                {
                    lock (DictToCopy.dictForThreadOrSession)
                    {
                        lock (dictForThreadOrSession)
                        {
                            Dictionary<Key, List<Value>>.Enumerator enumCopy = DictToCopy.dictMultiMap.GetEnumerator();

                            while (enumCopy.MoveNext())
                            {
                                List<Value> listValue = new List<Value>();
                                List<Value>.Enumerator enumList = enumCopy.Current.Value.GetEnumerator();

                                while (enumList.MoveNext())
                                {
                                    listValue.Add(enumList.Current);
                                }

                                dictMultiMap.Add(enumCopy.Current.Key, listValue);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);   
            }
        }

        /// <summary>
        /// Adds an element to the Multi map.
        /// </summary>
        /// <param name="KeyElement"></param>
        /// <param name="ValueElement"></param>
        public void Add(Key KeyElement, Value ValueElement)
        {
            try
            {
                List<Value> listToAdd = null;

                lock (dictForThreadOrSession)
                {
                    if (dictMultiMap.TryGetValue(KeyElement, out listToAdd))
                    {
                        listToAdd.Add(ValueElement);
                    }
                    else
                    {
                        listToAdd = new List<Value>();
                        listToAdd.Add(ValueElement);
                        dictMultiMap.Add(KeyElement, listToAdd);
                        listForEnumerator.Add(KeyElement); // Added for enumerating at Order(1)
                        lockDetailsForKeys.Add(KeyElement, new LockDetails<Key, Value>(this));
                    }
                }
            }
            catch(Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }
        }

        /// <summary>
        /// Gets the first Item in the Multi map.
        /// </summary>
        /// <param name="ItemToFind"></param>
        /// <returns></returns>
        public Value GetFirstItem(Key ItemToFind)
        {
            Value retVal = default(Value);
            try
            {
                List<Value> listItems = null;

                lock (dictForThreadOrSession)
                {
                    if (dictMultiMap.TryGetValue(ItemToFind, out listItems))
                    {
                        if (listItems.Count > 0)
                        {
                            retVal = listItems[0];

                            SetFirstItemRetrieved(ItemToFind);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retVal;

        }

        /// <summary>
        /// Gets the Next Item in Multi map.  If this method is called first, it returns first item.
        /// </summary>
        /// <param name="ItemToFind"></param>
        /// <returns></returns>
        public Value GetNextItem(Key ItemToFind)
        {
            Value retVal = default(Value);

            try
            {
                lock (dictForThreadOrSession)
                {
                    List<Value> listItems = null;
                    if (dictMultiMap.TryGetValue(ItemToFind, out listItems))
                    {
                        if (listItems.Count > 0)
                        {
                            int CounterToRetrieve = GetItemToBeRetrievedForCurrentThreadOrSession(ItemToFind);

                            if (CounterToRetrieve >= listItems.Count)
                            {
                                return retVal;
                            }

                            if (CounterToRetrieve == -1)
                            {
                                CounterToRetrieve = MoveToNextItemForThreadOrSession(ItemToFind);
                            }

                            retVal = listItems[CounterToRetrieve];
                           
                            MoveToNextItemForThreadOrSession(ItemToFind);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retVal;
        }
        /// <summary>
        /// Gets the Current value in the list of values for the searched Key
        /// </summary>
        /// <param name="ItemToFind"></param>
        /// <returns></returns>
        public Value GetCurrentItem(Key ItemToFind)
        {
            Value retVal = default(Value);

            try
            {
                lock (dictForThreadOrSession)
                {
                    List<Value> listItems = null;
                    if (dictMultiMap.TryGetValue(ItemToFind, out listItems))
                    {
                        if (listItems.Count > 0)
                        {
                            int CounterToRetrieve = GetItemToBeRetrievedForCurrentThreadOrSession(ItemToFind);

                            if (CounterToRetrieve >= listItems.Count)
                            {
                                return retVal;
                            }

                            retVal = listItems[CounterToRetrieve];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retVal;
        }

        /// <summary>
        /// Gets the Next Item in Multi map.  If this method is called first, it returns first item.
        /// </summary>
        /// <param name="ItemNumber"></param>
        /// <returns></returns>
        public KeyValuePair<Key,ValueItem<Key,Value>> GetNthItem(int ItemNumber)
        {
            KeyValuePair<Key, ValueItem<Key, Value>> retValue = default(KeyValuePair<Key, ValueItem<Key, Value>>);

            try
            {
                if (ItemNumber < 0) return retValue;

                lock (dictForThreadOrSession)
                {
                    if (ItemNumber < listForEnumerator.Count)
                    {
                        Key keyToSearchInDict = listForEnumerator[ItemNumber];

                        List<Value> listValueReturned= null;
                        if (dictMultiMap.TryGetValue(keyToSearchInDict, out listValueReturned))
                        {
                            retValue = new KeyValuePair<Key, ValueItem<Key, Value>>(keyToSearchInDict, new ValueItem<Key, Value>(this, keyToSearchInDict));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retValue;
        }

        /// <summary>
        /// Gets the Item in Multi map based on Key passed. 
        /// </summary>
        /// <param name="KeyItem"></param>
        /// <returns></returns>
        internal bool GetForKey(Key KeyItem, ref KeyValuePair<Key, ValueItem<Key, Value>> keyValuePair)
        {
            keyValuePair = default(KeyValuePair<Key, ValueItem<Key, Value>>);
            bool retValue = false;

            try
            {
                lock (dictForThreadOrSession)
                {
                    List<Value> listValueReturned = null;
                    if (dictMultiMap.TryGetValue(KeyItem, out listValueReturned))
                    {
                        LockDetails<Key, Value> lockDetails = null;

                        if (lockDetailsForKeys.TryGetValue(KeyItem, out lockDetails))
                        {
                            if (lockDetails.ItemMarkedForDeleteAll)
                            {
                                if (lockDetails.IsItemVisibleToThisThread(GetThreadOrSessionDetails().ThreadId))
                                {
                                    keyValuePair = new KeyValuePair<Key, ValueItem<Key, Value>>(KeyItem, new ValueItem<Key, Value>(this, KeyItem));
                                    retValue = true;
                                }
                                else
                                {
                                    //Key KeyItemNew = default(Key);
                                    //int StoredIndex = GetThreadOrSessionDetails().GetStoredIndex(ref KeyItemNew);
                                    //if (GetNthKey(StoredIndex, ref KeyItemNew))
                                    //{
                                    //    retValue = new KeyValuePair<Key, ValueItem<Key, Value>>(KeyItemNew, new ValueItem<Key, Value>(this, KeyItemNew));

                                    //}
                                }
                            }
                            else
                            {
                                keyValuePair = new KeyValuePair<Key, ValueItem<Key, Value>>(KeyItem, new ValueItem<Key, Value>(this, KeyItem));
                                retValue = true;
                            }

                        }

                    }
                    else
                    {
                        //Key KeyItemNew = default(Key);
                        //int StoredIndex = GetThreadOrSessionDetails().GetStoredIndex(ref KeyItemNew);
                        //if (GetNthKey(StoredIndex, ref KeyItemNew))
                        //{
                        //    retValue = new KeyValuePair<Key, ValueItem<Key, Value>>(KeyItemNew, new ValueItem<Key, Value>(this, KeyItemNew));

                        //}
                    }
                }

                
            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="NthKey"></param>
        /// <returns></returns>
        public bool GetNthKey(int Index, ref Key NthKey)
        {
            bool retValue = false;
            try
            {
                if (Index < 0) return retValue;

                lock (dictForThreadOrSession)
                {
                    LockDetails<Key,Value> lockDetails = null;

                    while (Index < listForEnumerator.Count)
                    {
                        if (lockDetailsForKeys.TryGetValue(listForEnumerator[Index], out lockDetails))
                        {
                            if (lockDetails.ItemMarkedForDeleteAll)
                            {
                                    Index++;
                            }
                            else
                            {
                                retValue = true;
                                NthKey = listForEnumerator[Index];
                                break;
                            }
                        }
                        else
                        {
                            break;
                            // If control comes here then thre is a bug.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retValue;
        }


        /// <summary>
        /// Move to next Value for the specified key
        /// </summary>
        /// <param name="ItemToFind"></param>
        /// <returns></returns>
        public bool MoveToNextItem(Key ItemToFind)
        {
            bool retVal = false;

            try
            {
                lock (dictForThreadOrSession)
                {
                    List<Value> listItems = null;
                    if (dictMultiMap.TryGetValue(ItemToFind, out listItems))
                    {
                        if (listItems.Count > 0)
                        {
                            int CounterToRetrieve = MoveToNextItemForThreadOrSession(ItemToFind);

                            if (CounterToRetrieve >= listItems.Count)
                            {
                                retVal = false;
                            }
                            else
                            {
                                retVal = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retVal;
        }

        /// <summary>
        /// Move the pointer to the first item for the specified key
        /// </summary>
        /// <param name="ItemToFind"></param>
        public void ResetToFirstItem(Key ItemToFind)
        {
            try
            {
                lock (dictForThreadOrSession)
                {
                    List<Value> listItems = null;
                    if (dictMultiMap.TryGetValue(ItemToFind, out listItems))
                    {
                        
                        if (listItems.Count > 0)
                        {
                            SetFirstItemRetrieved(ItemToFind);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

        }


        /// <summary>
        /// Iterates through all the values for the Key one by one.
        /// </summary>
        /// <param name="ItemToFind"></param>
        /// <returns></returns>
        public Value Iterate(Key ItemToFind)
        {
            return GetNextItem(ItemToFind);
        }

        /// <summary>
        /// Removes the Key and all the values for an item.
        /// </summary>
        /// <param name="KeyElement"></param>
        internal bool DeleteAll(Key KeyElement)
        {
            bool retVal = false;

            try
            {
                List<Value> listToRemove = null;

                lock (dictForThreadOrSession)
                {
                    if (dictMultiMap.TryGetValue(KeyElement, out listToRemove))
                    {
                        listToRemove.Clear();
                        dictMultiMap.Remove(KeyElement);
                        listForEnumerator.Remove(KeyElement);
                        lockDetailsForKeys.Remove(KeyElement);
                        retVal = true;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retVal;
        }

        /// <summary>
        /// Deletes one Key and one Value from the Multi map.
        /// </summary>
        /// <param name="KeyElement"></param>
        /// <param name="ValueElement"></param>
        internal bool Delete(Key KeyElement, Value ValueElement)
        {
            bool retVal = false;
            try
            {
                List<Value> listToRemove = null;

                lock (dictForThreadOrSession)
                {
                    if (dictMultiMap.TryGetValue(KeyElement, out listToRemove))
                    {
                        listToRemove.Remove(ValueElement);

                        if (listToRemove.Count == 0)
                        {
                            listToRemove = null;
                            retVal = dictMultiMap.Remove(KeyElement);
                            lockDetailsForKeys.Remove(KeyElement);   
                            listForEnumerator.Remove(KeyElement);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new MultiMapBKException(ex, ex.Message);
            }

            return retVal;
        }

        public void Remove(Key KeyToRemove)
        {
            MarkThisItemForDeletion(KeyToRemove);

        }

        public void Remove(Key KeyToRemove, int TimeOut)
        {
            AutoResetEvent eventToWait = null;
            MarkThisItemForDeletion(KeyToRemove, ref eventToWait);

            eventToWait.WaitOne(TimeOut, false);
        }

        public void Remove(Key KeyToRemove, Value ValueToRemove)
        {
            MarkThisItemForDeletion(KeyToRemove, ValueToRemove);

        }

        public void Remove(Key KeyToRemove, Value ValueToRemove, int TimeOut)
        {
            AutoResetEvent eventToWait = null;
            MarkThisItemForDeletion(KeyToRemove, ValueToRemove,ref eventToWait);

            eventToWait.WaitOne(TimeOut, false);
        }

        /// <summary>
        /// Clears all the elements in the collection.
        /// </summary>
        public void Clear()
        {
            lock (dictForThreadOrSession)
            {
                Dictionary<Key, List<Value>>.Enumerator enumDictElements = dictMultiMap.GetEnumerator();
                List<Key> listToRem = new List<Key>();

                while (enumDictElements.MoveNext())
                {
                    listToRem.Add(enumDictElements.Current.Key);
                    //enumDictElements.Current.Value.Clear();
                }

                List<Key>.Enumerator enumList = listToRem.GetEnumerator();

                while (enumList.MoveNext())
                {
                    this.MarkThisItemForDeletion(enumList.Current);
                }

                listToRem.Clear();
                dictForThreadOrSession.Clear();
                //dictMultiMap.Clear();
            }
        }

        /// <summary>
        /// Disposes all the objects stored in the collection.
        /// </summary>
        public void Dispose()
        {
            lock (dictForThreadOrSession)
            {
                if (!dispose)
                {
                    dispose = true;
                    DisposeAll();
                }
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~MultimapBK()
        {
            Dispose();
        }

        internal bool MarkThisItemForAccess(Key KeyItem)
        {
            LockDetails<Key, Value> lockDet = null;
            bool retVal = false;

            lock (dictForThreadOrSession)
            {
                if (lockDetailsForKeys.TryGetValue(KeyItem, out lockDet))
                {
                    lockDet.AddLock(KeyItem, GetThreadOrSessionDetails());
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }

            return retVal;
        }

        internal void ReleaseThisItemFromAccess(Key KeyItem)
        {
            LockDetails<Key, Value> lockDet = null;

            lock (dictForThreadOrSession)
            {
                if (lockDetailsForKeys.TryGetValue(KeyItem, out lockDet))
                {
                    lockDet.RemoveLock(KeyItem, GetThreadOrSessionDetails());
                }
            }
        }


        internal void MarkThisItemForDeletion(Key KeyItem)
        {
            LockDetails<Key, Value> lockDet = null;

            if (lockDetailsForKeys.TryGetValue(KeyItem, out lockDet))
            {
                lock (dictForThreadOrSession)
                {
                    ReleaseThisItemFromAccess(KeyItem);
                    lockDet.AddKeyToBeDeleted(KeyItem);
                }
            }
        }

        internal void MarkThisItemForDeletion(Key KeyItem, Value ValueItem)
        {
            LockDetails<Key, Value> lockDet = null;

            if (lockDetailsForKeys.TryGetValue(KeyItem, out lockDet))
            {
                lock (dictForThreadOrSession)
                {
                    ReleaseThisItemFromAccess(KeyItem);
                    lockDet.AddValueToBeDeleted(KeyItem, ValueItem);
                }
            }
        }

        internal void MarkThisItemForDeletion(Key KeyItem, ref AutoResetEvent EventToWait)
        {
            LockDetails<Key, Value> lockDet = null;

            if (lockDetailsForKeys.TryGetValue(KeyItem, out lockDet))
            {
                lock (dictForThreadOrSession)
                {
                    ReleaseThisItemFromAccess(KeyItem);
                    lockDet.AddKeyToBeDeleted(KeyItem);
                    EventToWait = lockDet.NotifyOnDeletionComplete();
                }
            }
        }

        internal void MarkThisItemForDeletion(Key KeyItem, Value ValueItem, ref AutoResetEvent EventToWait)
        {
            LockDetails<Key, Value> lockDet = null;

            if (lockDetailsForKeys.TryGetValue(KeyItem, out lockDet))
            {
                lock (dictForThreadOrSession)
                {
                    ReleaseThisItemFromAccess(KeyItem);
                    lockDet.AddValueToBeDeleted(KeyItem, ValueItem);
                    EventToWait = lockDet.NotifyOnDeletionComplete();
                }
            }
        }

        private void Initialize()
        {
            dispose = false;
            if (System.Web.HttpContext.Current == null)
            {
                thisIsNotWebProject = true;   // This is a web project.  
            }
            else
            {
                thisIsNotWebProject = false;  // This is not a web project.
            }

            dictMultiMap = new Dictionary<Key, List<Value>>();
            dictForThreadOrSession = new Dictionary<string, ThreadDetails<Key, Value>>();
            listForEnumerator = new List<Key>();
            lockDetailsForKeys = new Dictionary<Key, LockDetails<Key, Value>>();
        }

        private void SetFirstItemRetrieved(Key keyItem)
        {
            ThreadDetails<Key, Value> threadDetails = null;

            if (thisIsNotWebProject)
            {
                //if (dictForThreadOrSession.TryGetValue(Thread.CurrentThread.ManagedThreadId.ToString(), out threadDetails))
                //{
                //    dictForThreadOrSession.Remove(Thread.CurrentThread.ManagedThreadId.ToString());
                //}
                //else
                //{
                //    threadDetails = new ThreadDetails<Key, Value>(Thread.CurrentThread.ManagedThreadId.ToString(), default(Key), 0);
                //}
                threadDetails = GetThreadOrSessionDetails();
                threadDetails.ResetCount();
                threadDetails.IncrementCurrentCount(keyItem);
                threadDetails.IncrementCurrentCount(keyItem);
                threadDetails.LastSearchedItem = keyItem;

                //dictForThreadOrSession.Add(Thread.CurrentThread.ManagedThreadId.ToString(), threadDetails);
            }
            else
            {
                //if (dictForThreadOrSession.TryGetValue(HttpContext.Current.Session.SessionID, out threadDetails))
                //{
                //    dictForThreadOrSession.Remove(HttpContext.Current.Session.SessionID);
                //}
                //else
                //{
                //    threadDetails = new ThreadDetails<Key, Value>(HttpContext.Current.Session.SessionID, default(Key), 0);
                //}
                threadDetails = GetThreadOrSessionDetails();
                threadDetails.ResetCount();
                threadDetails.IncrementCurrentCount(keyItem);
                threadDetails.IncrementCurrentCount(keyItem);
                threadDetails.LastSearchedItem = keyItem;

                //dictForThreadOrSession.Add(HttpContext.Current.Session.SessionID, threadDetails);
            }

        }

        private ThreadDetails<Key, Value> GetThreadOrSessionDetails()
        {
            ThreadDetails<Key, Value> retVal = null;
            if (thisIsNotWebProject)
            {
                if (!dictForThreadOrSession.TryGetValue(Thread.CurrentThread.ManagedThreadId.ToString(), out retVal))
                {
                    retVal = new ThreadDetails<Key, Value>(Thread.CurrentThread.ManagedThreadId.ToString(),
                                                                        default(Key),-1);
                    dictForThreadOrSession.Add(Thread.CurrentThread.ManagedThreadId.ToString(), retVal);

                }
            }
            else
            {
                if (!dictForThreadOrSession.TryGetValue(HttpContext.Current.Session.SessionID, out retVal))
                {
                    retVal = new ThreadDetails<Key, Value>(HttpContext.Current.Session.SessionID,
                                                                    default(Key),-1);
                    dictForThreadOrSession.Add(HttpContext.Current.Session.SessionID, retVal);
                }
            }

            return retVal;
        }

        private int GetItemToBeRetrievedForCurrentThreadOrSession(Key KeyItem)
        {
            //ThreadDetails<Key, Value> threadDetails = null;

            //if (thisIsNotWebProject)
            //{
            //    dictForThreadOrSession.TryGetValue(Thread.CurrentThread.ManagedThreadId.ToString(), out threadDetails);
            //}
            //else
            //{
            //    dictForThreadOrSession.TryGetValue(HttpContext.Current.Session.SessionID, out threadDetails);
            //}

            //if (null == threadDetails) return -1;

            //return threadDetails.GetCurrentCount(KeyItem);

            return GetThreadOrSessionDetails().GetCurrentCount(KeyItem);
        }

        private int MoveToNextItemForThreadOrSession(Key keyItem)
        {
            ThreadDetails<Key, Value> threadDetails = null;

            if (thisIsNotWebProject)
            {
                //if (dictForThreadOrSession.TryGetValue(Thread.CurrentThread.ManagedThreadId.ToString(), out threadDetails))
                //{
                //    dictForThreadOrSession.Remove(Thread.CurrentThread.ManagedThreadId.ToString());
                //}
                //else
                //{
                //    threadDetails = new ThreadDetails<Key, Value>(Thread.CurrentThread.ManagedThreadId.ToString(),
                //                                            default(Key),
                //                                            -1);
                //}

                threadDetails = GetThreadOrSessionDetails();
                threadDetails.IncrementCurrentCount(keyItem);
                threadDetails.LastSearchedItem = keyItem;

                //dictForThreadOrSession.Add(Thread.CurrentThread.ManagedThreadId.ToString(), threadDetails);
            }
            else
            {
                //if (dictForThreadOrSession.TryGetValue(HttpContext.Current.Session.SessionID, out threadDetails))
                //{
                //    dictForThreadOrSession.Remove(HttpContext.Current.Session.SessionID);
                //}
                //else
                //{
                //    threadDetails = new ThreadDetails<Key, Value>(HttpContext.Current.Session.SessionID,
                //                                            default(Key),
                //                                            -1);
                //}
                threadDetails = GetThreadOrSessionDetails();
                threadDetails.IncrementCurrentCount(keyItem);
                threadDetails.LastSearchedItem = keyItem;

                //dictForThreadOrSession.Add(HttpContext.Current.Session.SessionID, threadDetails);
            }

            return threadDetails.Counter;
        }

        private void DisposeAll()
        {
            Dictionary<Key, List<Value>>.Enumerator enumDictElements = dictMultiMap.GetEnumerator();
            IDisposable disposableObject = null;

            while (enumDictElements.MoveNext())
            {
                List<Value>.Enumerator enumEachValues = enumDictElements.Current.Value.GetEnumerator();
                while (enumEachValues.MoveNext())
                {
                    try
                    {
                        disposableObject = (IDisposable)enumEachValues.Current;
                        if (null != disposableObject)
                        {
                            disposableObject.Dispose();
                        }

                    }
                    catch (InvalidCastException)
                    { // This is not a disposable object
                    }
                    catch (Exception ex)
                    {
                        throw new MultiMapBKException(ex, ex.Message);
                    }
                }

                enumDictElements.Current.Value.Clear();
                try
                {
                    disposableObject = (IDisposable)enumDictElements.Current.Key;
                    if (null != disposableObject)
                    {
                        disposableObject.Dispose();
                    }
                }
                catch (InvalidCastException)
                {
                    // This is not a disposable object
                }
                catch (Exception ex)
                {
                    throw new MultiMapBKException(ex, ex.Message);
                }

            }

            dictMultiMap.Clear();
        }

        internal void StoreCurrentItem(Key KeyToStore, int CurrentIndex)
        {
            ThreadDetails<Key, Value> threadDetails = GetThreadOrSessionDetails();

            threadDetails.StoreCurrentIndex(KeyToStore, CurrentIndex);

            //if (thisIsNotWebProject)
            //{
            //    if (dictForThreadOrSession.TryGetValue(Thread.CurrentThread.ManagedThreadId.ToString(), out threadDetails))
            //    {
            //        dictForThreadOrSession.Remove(Thread.CurrentThread.ManagedThreadId.ToString());
            //    }
            //    else
            //    {
            //        threadDetails = new ThreadDetails<Key, Value>(Thread.CurrentThread.ManagedThreadId.ToString(), default(Key), -1);
            //    }

            //    threadDetails.StoreCurrent(ItemToStore, CurrentIndex);
            //    dictForThreadOrSession.Add(Thread.CurrentThread.ManagedThreadId.ToString(), threadDetails);
            //}
            //else
            //{
            //    if (dictForThreadOrSession.TryGetValue(HttpContext.Current.Session.SessionID, out threadDetails))
            //    {
            //        dictForThreadOrSession.Remove(HttpContext.Current.Session.SessionID);
            //    }
            //    else
            //    {
            //        threadDetails = new ThreadDetails<Key, Value>(HttpContext.Current.Session.SessionID,
            //                                                default(Key),
            //                                                -1);
            //    }
            //    threadDetails.StoreCurrent(ItemToStore, CurrentIndex);

            //    dictForThreadOrSession.Add(HttpContext.Current.Session.SessionID, threadDetails);
            //}

        }

        internal int GetStoredIndex(ref Key StoredKey)
        {

            ThreadDetails<Key, Value> threadDetails = GetThreadOrSessionDetails();

            return threadDetails.GetStoredIndex(ref StoredKey);

            //if (thisIsNotWebProject)
            //{
            //    if (dictForThreadOrSession.TryGetValue(Thread.CurrentThread.ManagedThreadId.ToString(), out threadDetails))
            //    {
            //        threadDetails.GetStoredCurrent(ref refStoredItem, ref CurrentIndex);
            //        retValue = true;
            //    }
            //    else
            //    {
            //        dictForThreadOrSession.Add(Thread.CurrentThread.ManagedThreadId.ToString(), new ThreadDetails<Key,Value>(Thread.CurrentThread.ManagedThreadId.ToString(), default(Key), -1));
            //        CurrentIndex = 0;
            //        return false;
            //    }
            //}
            //else
            //{
            //    if (dictForThreadOrSession.TryGetValue(HttpContext.Current.Session.SessionID, out threadDetails))
            //    {
            //        threadDetails.GetStoredCurrent(ref refStoredItem, ref CurrentIndex);
            //        retValue = true;
            //    }
            //    else
            //    {
            //        dictForThreadOrSession.Add(HttpContext.Current.Session.SessionID, new ThreadDetails<Key,Value>(HttpContext.Current.Session.SessionID, default(Key), -1));
            //        CurrentIndex = 0;
            //        return false;
            //    }
            //}

        }

        IEnumerator<KeyValuePair<Key, ValueItem<Key, Value>>> IEnumerable<KeyValuePair<Key, ValueItem<Key, Value>>>.GetEnumerator()
        {
            return new MultimapEnum(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)new MultimapEnum(this);
        }


        /// <summary>
        /// Enumerator class for the Multi map
        /// </summary>
        public class MultimapEnum: IEnumerator<KeyValuePair<Key, ValueItem<Key, Value>>>
        {
            private MultimapBK<Key, Value> multiMap;

            internal MultimapEnum(MultimapBK<Key, Value> MultiMapParam)
            {
                multiMap = MultiMapParam;
            }

            private KeyValuePair<Key, ValueItem<Key, Value>> GetCurrentItem(int CurrentItem)
            {
                //if (CurrentItem == -1)
                //{
                //    CurrentItem = 0; // Let's be gentle to give back first item.
                //}

                return multiMap.GetNthItem(CurrentItem);
            }

            KeyValuePair<Key, ValueItem<Key, Value>> IEnumerator<KeyValuePair<Key, ValueItem<Key, Value>>>.Current
            {
                get
                {
                    return this.Current;
                }
            }

            /// <summary>
            /// Returns Current Element
            /// </summary>
            public KeyValuePair<Key, ValueItem<Key, Value>> Current
            {
                get
                {
                    KeyValuePair<Key, ValueItem<Key, Value>> retValue = new KeyValuePair<Key, ValueItem<Key, Value>>();
                    Key CurrentKeyStored = default(Key);
                    int CurrentIndex = multiMap.GetStoredIndex(ref CurrentKeyStored);

                    if (CurrentIndex == -1)
                    {
                        // Let's be gentle and return empty;
                        return retValue;
                    }

                    multiMap.GetForKey(CurrentKeyStored, ref retValue);

                    return retValue;                    
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (object) this.Current;
                }
            }

            /// <summary>
            /// Move to the next Item
            /// </summary>
            /// <returns></returns>

            public bool MoveNext()
            {
                Key KeyForIndex = default(Key);
                Key CurrentKeyStored = default(Key);
                bool retVal = false, releaseAccess = true;
                int CurrentIndex = multiMap.GetStoredIndex(ref CurrentKeyStored);
                if (CurrentIndex == -1)
                {
                    releaseAccess = false;
                    // Firt Item.  so no need to release access;
                }
                else
                {
                    KeyValuePair<Key, ValueItem<Key, Value>> keyValRet = default(KeyValuePair<Key, ValueItem<Key, Value>>);
                    if (!multiMap.GetForKey(CurrentKeyStored, ref keyValRet) )
                    {
                        CurrentIndex--;
                    }
                }
                CurrentIndex++;


                while (multiMap.GetNthKey(CurrentIndex, ref KeyForIndex))
                {
                    if (!multiMap.MarkThisItemForAccess(KeyForIndex))
                    {
                        CurrentIndex++;
                    }
                    else
                    {
                        retVal = true;
                        multiMap.StoreCurrentItem(KeyForIndex, CurrentIndex);
                        break;
                    }
                }

                if (releaseAccess)
                {
                    multiMap.ReleaseThisItemFromAccess(CurrentKeyStored);
                }
                return retVal;
            }

            /// <summary>
            /// Reset to the first Item
            /// </summary>
            public void Reset()
            {
                Key CurrentKeyStored = default(Key);
                int CurrentIndex = multiMap.GetStoredIndex(ref CurrentKeyStored);
                if (CurrentIndex != -1)
                {
                    multiMap.ReleaseThisItemFromAccess(CurrentKeyStored);
                }
                multiMap.StoreCurrentItem(default(Key), -1);

            }

            /// <summary>
            /// Dispose the object
            /// </summary>
            public void Dispose()
            {
                // Since outer class is disposing, we dont need to here.
            }


        }

    }
}
