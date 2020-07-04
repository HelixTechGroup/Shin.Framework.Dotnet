#region Usings
#endregion

#region Usings
using System;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Shin.Framework.Collections
{
    public sealed class ListDictionary<TKey, TValue> : IDictionary<TKey, IList<TValue>>
    {
        #region Members
        readonly Dictionary<TKey, IList<TValue>> m_innerValues = new Dictionary<TKey, IList<TValue>>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the list associated with the given key. The
        /// access always succeeds, eventually returning an empty list.
        /// </summary>
        /// <param name="key">The key of the list to access.</param>
        /// <returns>The list associated with the key.</returns>
        public IList<TValue> this[TKey key]
        {
            get
            {
                if (!m_innerValues.ContainsKey(key))
                    m_innerValues.Add(key, new List<TValue>());
                return m_innerValues[key];
            }
            set { m_innerValues[key] = value; }
        }

        /// <summary>
        /// Gets the number of lists in the dictionary.
        /// </summary>
        /// <value>Value indicating the values count.</value>
        public int Count
        {
            get { return m_innerValues.Count; }
        }

        /// <summary>
        /// Gets the list of keys in the dictionary.
        /// </summary>
        /// <value>Collection of keys.</value>
        public ICollection<TKey> Keys
        {
            get { return m_innerValues.Keys; }
        }

        /// <summary>
        /// Gets a shallow copy of all values in all lists.
        /// </summary>
        /// <value>List of values.</value>
        public IList<TValue> Values
        {
            get
            {
                var values = new List<TValue>();
                foreach (IEnumerable<TValue> list in m_innerValues.Values)
                    values.AddRange(list);

                return values;
            }
        }

        /// <summary>
        /// See <see cref="ICollection{TValue}.IsReadOnly"/> for more information.
        /// </summary>
        bool ICollection<KeyValuePair<TKey, IList<TValue>>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<TKey, IList<TValue>>>)m_innerValues).IsReadOnly; }
        }

        /// <summary>
        /// See <see cref="IDictionary{TKey,TValue}.Values"/> for more information.
        /// </summary>
        ICollection<IList<TValue>> IDictionary<TKey, IList<TValue>>.Values
        {
            get { return m_innerValues.Values; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// If a list does not already exist, it will be created automatically.
        /// </summary>
        /// <param name="key">The key of the list that will hold the value.</param>
        public void Add(TKey key)
        {
            Throw.IfNull(key).ArgumentNullException(nameof(key));

            CreateNewList(key);
        }

        /// <summary>
        /// Adds a value to a list with the given key. If a list does not already exist,
        /// it will be created automatically.
        /// </summary>
        /// <param name="key">The key of the list that will hold the value.</param>
        /// <param name="value">The value to add to the list under the given key.</param>
        public void Add(TKey key, TValue value)
        {
            Throw.IfNull(key).ArgumentNullException(nameof(key));
            Throw.IfNull(value).ArgumentNullException(nameof(value));

            if (m_innerValues.ContainsKey(key))
                m_innerValues[key].Add(value);
            else
            {
                var values = CreateNewList(key);
                values.Add(value);
            }
        }

        /// <summary>
        /// Removes all entries in the dictionary.
        /// </summary>
        public void Clear()
        {
            m_innerValues.Clear();
        }

        /// <summary>
        /// Determines whether the dictionary contains the specified value.
        /// </summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>true if the dictionary contains the value in any list; otherwise, false.</returns>
        public bool ContainsValue(TValue value)
        {
            foreach (var pair in m_innerValues)
            {
                if (pair.Value.Contains(value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the dictionary contains the given key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>true if the dictionary contains the given key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            Throw.IfNull(key).ArgumentNullException(nameof(key));

            return m_innerValues.ContainsKey(key);
        }

        /// <summary>
        /// Retrieves the all the elements from the list which have a key that matches the condition
        /// defined by the specified predicate.
        /// </summary>
        /// <param name="keyFilter">The filter with the condition to use to filter lists by their key.</param>
        /// <returns>The elements that have a key that matches the condition defined by the specified predicate.</returns>
        public IEnumerable<TValue> FindAllValuesByKey(Predicate<TKey> keyFilter)
        {
            foreach (var pair in this)
            {
                if (keyFilter(pair.Key))
                {
                    foreach (var value in pair.Value)
                        yield return value;
                }
            }
        }

        /// <summary>
        /// Retrieves all the elements that match the condition defined by the specified predicate.
        /// </summary>
        /// <param name="valueFilter">The filter with the condition to use to filter values.</param>
        /// <returns>The elements that match the condition defined by the specified predicate.</returns>
        public IEnumerable<TValue> FindAllValues(Predicate<TValue> valueFilter)
        {
            foreach (var pair in this)
            {
                foreach (var value in pair.Value)
                {
                    if (valueFilter(value))
                        yield return value;
                }
            }
        }

        /// <summary>
        /// Removes a list by key.
        /// </summary>
        /// <param name="key">The key of the list to remove.</param>
        /// <returns><see langword="true" /> if the element was removed.</returns>
        public bool Remove(TKey key)
        {
            Throw.IfNull(key).ArgumentNullException(nameof(key));

            return m_innerValues.Remove(key);
        }

        /// <summary>
        /// Removes a value from the list with the given key.
        /// </summary>
        /// <param name="key">The key of the list where the value exists.</param>
        /// <param name="value">The value to remove.</param>
        public void Remove(TKey key, TValue value)
        {
            Throw.IfNull(key).ArgumentNullException(nameof(key));
            Throw.IfNull(value).ArgumentNullException(nameof(value));

            if (m_innerValues.ContainsKey(key))
            {
                var innerList = (List<TValue>)m_innerValues[key];
                innerList.RemoveAll(delegate(TValue item) { return value.Equals(item); });
            }
        }

        /// <summary>
        /// Removes a value from all lists where it may be found.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        public void Remove(TValue value)
        {
            foreach (var pair in m_innerValues)
                Remove(pair.Key, value);
        }

        private List<TValue> CreateNewList(TKey key)
        {
            var values = new List<TValue>();
            m_innerValues.Add(key, values);

            return values;
        }

        /// <summary>
        /// See <see cref="IDictionary{TKey,TValue}.Add"/> for more information.
        /// </summary>
        void IDictionary<TKey, IList<TValue>>.Add(TKey key, IList<TValue> value)
        {
            Throw.IfNull(key).ArgumentNullException(nameof(key));
            Throw.IfNull(value).ArgumentNullException(nameof(value));

            m_innerValues.Add(key, value);
        }

        /// <summary>
        /// See <see cref="IDictionary{TKey,TValue}.TryGetValue"/> for more information.
        /// </summary>
        bool IDictionary<TKey, IList<TValue>>.TryGetValue(TKey key, out IList<TValue> value)
        {
            value = this[key];
            return true;
        }

        /// <summary>
        /// See <see cref="ICollection{TValue}.Add"/> for more information.
        /// </summary>
        void ICollection<KeyValuePair<TKey, IList<TValue>>>.Add(KeyValuePair<TKey, IList<TValue>> item)
        {
            ((ICollection<KeyValuePair<TKey, IList<TValue>>>)m_innerValues).Add(item);
        }

        /// <summary>
        /// See <see cref="ICollection{TValue}.Contains"/> for more information.
        /// </summary>
        bool ICollection<KeyValuePair<TKey, IList<TValue>>>.Contains(KeyValuePair<TKey, IList<TValue>> item)
        {
            return ((ICollection<KeyValuePair<TKey, IList<TValue>>>)m_innerValues).Contains(item);
        }

        /// <summary>
        /// See <see cref="ICollection{TValue}.CopyTo"/> for more information.
        /// </summary>
        void ICollection<KeyValuePair<TKey, IList<TValue>>>.CopyTo(KeyValuePair<TKey, IList<TValue>>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, IList<TValue>>>)m_innerValues).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// See <see cref="ICollection{TValue}.Remove"/> for more information.
        /// </summary>
        bool ICollection<KeyValuePair<TKey, IList<TValue>>>.Remove(KeyValuePair<TKey, IList<TValue>> item)
        {
            return ((ICollection<KeyValuePair<TKey, IList<TValue>>>)m_innerValues).Remove(item);
        }

        /// <summary>
        /// See <see cref="IEnumerable{TValue}.GetEnumerator"/> for more information.
        /// </summary>
        IEnumerator<KeyValuePair<TKey, IList<TValue>>> IEnumerable<KeyValuePair<TKey, IList<TValue>>>.GetEnumerator()
        {
            return m_innerValues.GetEnumerator();
        }

        /// <summary>
        /// See <see cref="System.Collections.IEnumerable.GetEnumerator"/> for more information.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_innerValues.GetEnumerator();
        }
        #endregion
    }
}