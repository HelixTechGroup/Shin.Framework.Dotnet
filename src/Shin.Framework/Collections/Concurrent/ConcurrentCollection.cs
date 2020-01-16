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
    public class ConcurrentCollection<T> : IList<T>,
                                           IReadOnlyList<T>,
                                           IList
    {
        #region Members
        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private T[] m_arr;
        private int m_count;

        private bool m_isFixedSize;
        private bool m_isReadOnly;
        private bool m_isSynchronized;

        [NonSerialized]
        private object m_syncRoot;
        #endregion

        #region Properties
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
            get
            {
                // There is no IList<T>.IsFixedSize, so we must assume that only
                // readonly collections are fixed size, if our internal item 
                // collection does not implement IList.  Note that Array implements
                // IList, and therefore T[] and U[] will be fixed-size.
                IList list = m_arr;
                if (list != null) return list.IsFixedSize;
                return m_arr.IsReadOnly;
            }
        }

        public bool IsReadOnly
        {
            get { return m_isReadOnly; }
        }

        public bool IsSynchronized
        {
            get { return m_isSynchronized; }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

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

        protected IList<T> Items
        {
            get { return m_arr; }
        }

        public object SyncRoot
        {
            get { return m_syncRoot; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (m_syncRoot == null)
                {
                    ICollection c = m_arr;
                    if (c != null)
                        m_syncRoot = c.SyncRoot;
                    else
                        Interlocked.CompareExchange<object>(ref m_syncRoot, new object(), null);
                }

                return m_syncRoot;
            }
        }
        #endregion

        public ConcurrentCollection(IList<T> list)
        {
            //if (list == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list);
            //}
            //items = list;
        }

        #region Methods
        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return value is T || value == null && default(T) == null;
        }

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
                Throw.If(array == null).ArgumentNullException(nameof(array));
                Throw.If(array.Rank != 1).ArgumentException(nameof(array));
                Throw.If(array.GetLowerBound(0) != 0).ArgumentException(nameof(array));
                Throw.If(arrayIndex < 0).ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex);
                Throw.If(m_count > array.Length - arrayIndex).ArgumentException(nameof(array), "Destination array was not long enough.");

                Array.Copy(m_arr, 0, array, arrayIndex, m_count);
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public virtual void DoSync(Action<ConcurrentCollection<T>> action)
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

        public virtual TResult GetSync<TResult>(Func<ConcurrentCollection<T>, TResult> func)
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
            m_lock.EnterUpgradeableReadLock();

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

        protected virtual void ClearItems()
        {
            Array.Clear(m_arr, m_count, 1);
        }

        protected virtual void InsertItem(int index, T item)
        {
            Insert(index, item);
        }

        protected virtual void RemoveItem(int index)
        {
            RemoveAt(index);
        }

        protected virtual void SetItem(int index, T item)
        {
            this[index] = item;
        }

        public int Add(object value)
        {
            Add((T)value);
            return IndexOf(value);
        }

        public bool Contains(object value)
        {
            if (IsCompatibleObject(value)) return Contains((T)value);
            return false;
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

        public void CopyTo(Array array, int index)
        {
            if (array is T[] tArray)
                CopyTo(tArray, index);
            else
            {
                //
                // Catch the obvious case assignment will fail.
                // We can found all possible problems by doing the check though.
                // For example, if the element type of the Array is derived from T,
                // we can't figure out if we can successfully copy the element beforehand.
                //
                var targetType = array.GetType().GetElementType();
                var sourceType = typeof(T);
                Throw.If(!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))).ArgumentException(nameof(sourceType));

                //
                // We can't cast array of value type to object[], so we don't support 
                // widening of primitive types here.
                //
                var objects = array as object[];
                Throw.If(objects == null).ArgumentException(nameof(array));

                var count = Count;
                try
                {
                    for (var i = 0; i < count; i++) objects[index++] = this[i];
                }
                catch (ArrayTypeMismatchException)
                {
                    Throw.Exception().ArgumentException(nameof(array));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Constructors
        public ConcurrentCollection(int initialCapacity)
        {
            m_arr = new T[initialCapacity];
        }

        public ConcurrentCollection() : this(4) { }

        public ConcurrentCollection(IEnumerable<T> items)
        {
            Throw.IfNull(m_arr).InvalidOperationException();

            m_arr = items.ToArray();
            m_count = m_arr.Length;
        }
        #endregion
    }
}