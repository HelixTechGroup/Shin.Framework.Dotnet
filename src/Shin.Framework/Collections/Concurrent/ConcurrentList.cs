#region Header
// // ----------------------------------------------------------------------
// // filename: ConcurrentList.cs
// // company: HelixTechGroup, LLC
// // date: 05-10-2017
// // namespace: Shield.Framework.Collections
// // class: ConcurrentList<T> : IList<T>, IDispose
// // summary: Class representing a ConcurrentList<T> : IList<T>, IDispose entity.
// // legal: Copyright (c) 2017 All Right Reserved
// // ------------------------------------------------------------------------
// 
#endregion

#region Usings
#endregion

#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#endregion

namespace Shin.Framework.Collections.Concurrent
{
    public class ConcurrentList : IList
    {
        #region Members
        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private object[] m_arr;
        private int m_count;
        #endregion

        #region Properties
        public virtual object this[int index]
        {
            get
            {
                m_lock.EnterReadLock();
                try
                {
                    Throw.If(index >= m_count).ArgumentOutOfRangeException(nameof(index), m_count);

                    return m_arr[index];
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
            set
            {
                m_lock.EnterUpgradeableReadLock();
                try
                {
                    m_lock.EnterWriteLock();
                    try
                    {
                        Throw.If(index >= m_count).ArgumentOutOfRangeException(nameof(index), m_count);

                        m_arr[index] = value;
                    }
                    finally
                    {
                        m_lock.ExitWriteLock();
                    }
                }
                finally
                {
                    m_lock.ExitUpgradeableReadLock();
                }
            }
        }

        public virtual int Count
        {
            get
            {
                m_lock.EnterReadLock();
                try
                {
                    return m_count;
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
        }

        public virtual int InternalArrayLength
        {
            get
            {
                m_lock.EnterReadLock();
                try
                {
                    return m_arr.Length;
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
        }

        public bool IsFixedSize
        {
            get { return m_arr.IsFixedSize; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return m_arr.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return m_arr.SyncRoot; }
        }
        #endregion

        #region Methods
        public virtual int Add(object value)
        {
            m_lock.EnterWriteLock();
            try
            {
                var newCount = m_count + 1;
                EnsureCapacity(newCount);
                m_arr[m_count] = value;
                m_count = newCount;
                return m_count;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public virtual void AddRange(ICollection items)
        {
            Throw.IfNull(items).ArgumentNullException(nameof(items));

            m_lock.EnterWriteLock();
            try
            {
                var newCount = m_count + items.Count;
                EnsureCapacity(newCount);
                var arr = new object[items.Count];
                items.CopyTo(arr, 0);
                Array.Copy(arr, 0, m_arr, m_count, items.Count);
                m_count = newCount;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public virtual void Clear()
        {
            m_lock.EnterWriteLock();
            try
            {
                Array.Clear(m_arr, 0, m_count);
                m_count = 0;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public virtual bool Contains(object value)
        {
            m_lock.EnterReadLock();
            try
            {
                return IndexOfInternal(value) != -1;
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual void CopyTo(Array array, int index)
        {
            m_lock.EnterReadLock();
            try
            {
                Throw.If(m_count > array.Length - index).ArgumentException(nameof(array), "Destination array was not long enough.");

                Array.Copy(m_arr, 0, array, index, m_count);
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual void DoSync(Action<ConcurrentList> action)
        {
            GetSync(l =>
                    {
                        action(l);
                        return 0;
                    });
        }

        public virtual IEnumerator GetEnumerator()
        {
            m_lock.EnterReadLock();

            try
            {
                for (var i = 0; i < m_count; i++)

                    // deadlocking potential mitigated by lock recursion enforcement
                    yield return m_arr[i];
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual TResult GetSync<TResult>(Func<ConcurrentList, TResult> func)
        {
            m_lock.EnterWriteLock();
            try
            {
                return func(this);
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public virtual int IndexOf(object value)
        {
            m_lock.EnterReadLock();
            try
            {
                return IndexOfInternal(value);
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual void Insert(int index, object value)
        {
            m_lock.EnterUpgradeableReadLock();

            try
            {
                Throw.If(index > m_count).ArgumentOutOfRangeException(nameof(index), m_count);

                m_lock.EnterWriteLock();
                try
                {
                    var newCount = m_count + 1;
                    EnsureCapacity(newCount);

                    // shift everything right by one, starting at index
                    Array.Copy(m_arr, index, m_arr, index + 1, m_count - index);

                    // insert
                    m_arr[index] = value;
                    m_count = newCount;
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
            finally
            {
                m_lock.ExitUpgradeableReadLock();
            }
        }

        public virtual void Remove(object value)
        {
            m_lock.EnterUpgradeableReadLock();

            try
            {
                var i = IndexOfInternal(value);

                if (i == -1)
                    return;

                m_lock.EnterWriteLock();
                try
                {
                    RemoveAtInternal(i);
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
            finally
            {
                m_lock.ExitUpgradeableReadLock();
            }
        }

        public virtual void RemoveAt(int index)
        {
            m_lock.EnterUpgradeableReadLock();
            try
            {
                Throw.If(index >= m_count).ArgumentOutOfRangeException(nameof(index), m_count);

                m_lock.EnterWriteLock();
                try
                {
                    RemoveAtInternal(index);
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
            finally
            {
                m_lock.ExitUpgradeableReadLock();
            }
        }

        protected virtual void EnsureCapacity(int capacity)
        {
            if (m_arr.Length >= capacity)
                return;

            int doubled;
            checked
            {
                try
                {
                    doubled = m_arr.Length * 2;
                }
                catch (OverflowException)
                {
                    doubled = int.MaxValue;
                }
            }

            var newLength = Math.Max(doubled, capacity);
            Array.Resize(ref m_arr, newLength);
        }

        protected virtual int IndexOfInternal(object item)
        {
            return Array.FindIndex(m_arr, 0, m_count, x => x.Equals(item));
        }

        protected virtual void RemoveAtInternal(int index)
        {
            Array.Copy(m_arr, index + 1, m_arr, index, m_count - index - 1);
            m_count--;

            // release last element
            Array.Clear(m_arr, m_count, 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Constructors
        public ConcurrentList(int initialCapacity)
        {
            m_arr = new object[initialCapacity];
        }

        public ConcurrentList() : this(4) { }

        public ConcurrentList(ICollection items)
        {
            Throw.IfNull(m_arr).InvalidOperationException();

            var arr = new object[items.Count];
            items.CopyTo(arr, 0);
            m_arr = arr;
            m_count = m_arr.Length;
        }
        #endregion
    }

    public class ConcurrentList<T> : IList<T>,
                                     IReadOnlyList<T>,
                                     IList
    {
        #region Members
        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private T[] m_arr;
        private int m_count;
        private bool m_isFixedSize;
        private bool m_isSynchronized;
        private object m_syncRoot;
        #endregion

        #region Properties
        public virtual T this[int index]
        {
            get
            {
                m_lock.EnterReadLock();
                try
                {
                    Throw.If(index >= m_count).ArgumentOutOfRangeException(nameof(index), m_count);

                    return m_arr[index];
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
            set
            {
                m_lock.EnterUpgradeableReadLock();
                try
                {
                    Throw.If(index >= m_count).ArgumentOutOfRangeException(nameof(index), m_count);

                    m_lock.EnterWriteLock();
                    try
                    {
                        m_arr[index] = value;
                    }
                    finally
                    {
                        m_lock.ExitWriteLock();
                    }
                }
                finally
                {
                    m_lock.ExitUpgradeableReadLock();
                }
            }
        }

        public virtual int Count
        {
            get
            {
                m_lock.EnterReadLock();
                try
                {
                    return m_count;
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
        }

        public virtual int InternalArrayLength
        {
            get
            {
                m_lock.EnterReadLock();
                try
                {
                    return m_arr.Length;
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
        }

        public bool IsFixedSize
        {
            get { return m_isFixedSize; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return m_isSynchronized; }
        }

        public object SyncRoot
        {
            get { return m_syncRoot; }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }
        #endregion

        #region Methods
        public virtual void Add(T item)
        {
            m_lock.EnterWriteLock();
            try
            {
                var newCount = m_count + 1;
                EnsureCapacity(newCount);
                m_arr[m_count] = item;
                m_count = newCount;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            Throw.IfNull(items).ArgumentNullException(nameof(items));

            m_lock.EnterWriteLock();
            try
            {
                var arr = items as T[] ?? items.ToArray();
                var newCount = m_count + arr.Length;
                EnsureCapacity(newCount);
                Array.Copy(arr, 0, m_arr, m_count, arr.Length);
                m_count = newCount;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public virtual void Clear()
        {
            m_lock.EnterWriteLock();
            try
            {
                Array.Clear(m_arr, 0, m_count);
                m_count = 0;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public virtual bool Contains(T item)
        {
            m_lock.EnterReadLock();
            try
            {
                return IndexOfInternal(item) != -1;
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            m_lock.EnterReadLock();
            try
            {
                Throw.If(m_count > array.Length - arrayIndex).ArgumentException(nameof(array), "Destination array was not long enough.");

                Array.Copy(m_arr, 0, array, arrayIndex, m_count);
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual void DoSync(Action<ConcurrentList<T>> action)
        {
            GetSync(l =>
                    {
                        action(l);
                        return 0;
                    });
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            m_lock.EnterReadLock();

            try
            {
                for (var i = 0; i < m_count; i++)

                    // deadlocking potential mitigated by lock recursion enforcement
                    yield return m_arr[i];
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual TResult GetSync<TResult>(Func<ConcurrentList<T>, TResult> func)
        {
            m_lock.EnterWriteLock();
            try
            {
                return func(this);
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public virtual int IndexOf(T item)
        {
            m_lock.EnterReadLock();
            try
            {
                return IndexOfInternal(item);
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual void Insert(int index, T item)
        {
            m_lock.EnterUpgradeableReadLock();

            try
            {
                Throw.If(index > m_count).ArgumentOutOfRangeException(nameof(index), m_count);

                m_lock.EnterWriteLock();
                try
                {
                    var newCount = m_count + 1;
                    EnsureCapacity(newCount);

                    // shift everything right by one, starting at index
                    Array.Copy(m_arr, index, m_arr, index + 1, m_count - index);

                    // insert
                    m_arr[index] = item;
                    m_count = newCount;
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
            finally
            {
                m_lock.ExitUpgradeableReadLock();
            }
        }

        public virtual bool Remove(T item)
        {
            //if (m_lock.IsUpgradeableReadLockHeld)
            //{
            //    while(m_lock.IsUpgradeableReadLockHeld)
            //    {
            //        Thread.Sleep(250);
            //    }
            //}

            //if (m_lock.IsReadLockHeld)
            //{
            //    while (m_lock.IsReadLockHeld)
            //    {
            //        Thread.Sleep(250);
            //    }
            //}

            m_lock.TryEnterUpgradeableReadLock(1000);

            try
            {
                var i = IndexOfInternal(item);

                if (i == -1)
                    return false;

                m_lock.EnterWriteLock();
                try
                {
                    RemoveAtInternal(i);
                    return true;
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
            finally
            {
                m_lock.ExitUpgradeableReadLock();
            }
        }

        public virtual void RemoveAt(int index)
        {
            m_lock.EnterUpgradeableReadLock();
            try
            {
                Throw.If(index >= m_count).ArgumentOutOfRangeException(nameof(index), m_count);

                m_lock.EnterWriteLock();
                try
                {
                    RemoveAtInternal(index);
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
            finally
            {
                m_lock.ExitUpgradeableReadLock();
            }
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        public int Add(object value)
        {
            Add((T)value);
            return IndexOf(value);
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        protected virtual void EnsureCapacity(int capacity)
        {
            if (m_arr.Length >= capacity)
                return;

            int doubled;
            checked
            {
                try
                {
                    doubled = m_arr.Length * 2;
                }
                catch (OverflowException)
                {
                    doubled = int.MaxValue;
                }
            }

            var newLength = Math.Max(doubled, capacity);
            Array.Resize(ref m_arr, newLength);
        }

        protected virtual int IndexOfInternal(T item)
        {
            return Array.FindIndex(m_arr, 0, m_count, x => x.Equals(item));
        }

        protected virtual void RemoveAtInternal(int index)
        {
            Array.Copy(m_arr, index + 1, m_arr, index, m_count - index - 1);
            m_count--;

            // release last element
            Array.Clear(m_arr, m_count, 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Constructors
        public ConcurrentList(int initialCapacity)
        {
            m_arr = new T[initialCapacity];
        }

        public ConcurrentList() : this(4) { }

        public ConcurrentList(IEnumerable<T> items)
        {
            Throw.IfNull(m_arr).InvalidOperationException();

            m_arr = items.ToArray();
            m_count = m_arr.Length;
        }
        #endregion
    }
}