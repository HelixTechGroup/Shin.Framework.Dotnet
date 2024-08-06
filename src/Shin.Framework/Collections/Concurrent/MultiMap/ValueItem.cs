using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BK.Util
{
    /// <summary>
    /// Contains Key and Value(s). i.e., Value.First, Value.Next and so on
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class ValueItem<Key, Value> where Key: IComparable<Key>
    {
        private MultimapBK<Key, Value> multiMap;
        private Key key;


        internal ValueItem(MultimapBK<Key, Value> multiMapItem, Key KeyItem)
        {
            key = KeyItem;
            multiMap = multiMapItem;
        }

        
        /// <summary>
        /// Returns the first Item
        /// </summary>
        public Value First
        {
            get
            {
                return multiMap.GetFirstItem(key);
            }
        }
        /// <summary>
        /// Move to Next Item
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            return multiMap.MoveToNextItem(key);
        }

        /// <summary>
        /// Return Current Item
        /// </summary>
        public Value Current
        {
            get
            {
                return multiMap.GetCurrentItem(key);
            }
        }

        /// <summary>
        /// Reset to first Item. 
        /// </summary>
        public void Reset()
        {
            multiMap.ResetToFirstItem(key);
        }

        /// <summary>
        /// Returns the Next Item
        /// </summary>
        public Value Next
        {
            get
            {
                return multiMap.GetNextItem(key);
            }
        }
    }
}
