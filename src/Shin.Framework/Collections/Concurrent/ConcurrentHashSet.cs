#region Usings
#endregion

#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
#endregion

namespace Shin.Framework.Collections.Concurrent
{
    public class ConcurrentHashSet<T> : ISet<T>,
                                        IReadOnlyCollection<T>,
                                        IDeserializationCallback,
                                        ISerializable
    {
        #region Members
        private readonly HashSet<T> m_hashSet;
        private readonly ReaderWriterLockSlim m_lock;
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                try
                {
                    m_lock.EnterReadLock();
                    return m_hashSet.Count;
                }
                finally
                {
                    if (m_lock.IsReadLockHeld)
                        m_lock.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        #endregion

        public ConcurrentHashSet()
        {
            m_hashSet = new HashSet<T>();
            m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public ConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            m_hashSet = new HashSet<T>(comparer);
            m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        #region Methods
        public bool Add(T item)
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.Add(item);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                m_hashSet.UnionWith(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                m_hashSet.IntersectWith(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                m_hashSet.ExceptWith(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                m_hashSet.SymmetricExceptWith(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.IsSubsetOf(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.IsSupersetOf(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.IsProperSupersetOf(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.IsProperSubsetOf(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.Overlaps(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.SetEquals(other);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                m_lock.EnterWriteLock();
                m_hashSet.Clear();
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            try
            {
                m_lock.EnterReadLock();
                return m_hashSet.Contains(item);
            }
            finally
            {
                if (m_lock.IsReadLockHeld)
                    m_lock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            try
            {
                m_lock.EnterWriteLock();
                m_hashSet.CopyTo(array, arrayIndex);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public bool Remove(T item)
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.Remove(item);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            try
            {
                m_lock.EnterWriteLock();
                return m_hashSet.GetEnumerator();
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public void OnDeserialization(object sender)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item)
        {
            try
            {
                m_lock.EnterWriteLock();
                m_hashSet.Add(item);
            }
            finally
            {
                if (m_lock.IsWriteLockHeld)
                    m_lock.ExitWriteLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}