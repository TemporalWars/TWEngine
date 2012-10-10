using System;
using System.Collections;
using System.Collections.Generic;

namespace ImageNexus.BenScharbach.TWTools.ParallelTasksComponent
{
    ///<summary>
    /// The <see cref="ThreadSafeDictionary{TKey, TValue}"/> class, inherits from the <see cref="IDictionary"/>, but
    /// provides thread safe operations via internal thread locks; also, provides new methods like <see cref="SafeAdd"/>.
    ///</summary>
    ///<typeparam name="TKey">Generic key to use</typeparam>
    ///<typeparam name="TValue">Generic value to store</typeparam>
    public class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        //This is the internal dictionary that we are wrapping
        private readonly IDictionary<TKey, TValue> _dict;
        
        //setup the lock;
        private readonly object _dictionaryLock = new object();
       
        // 
        ///<summary>
        /// Default contructor, which creates the internal <see cref="Dictionary{TKey, TValue}"/>.
        ///</summary>
        public ThreadSafeDictionary()
        {
            _dict = new Dictionary<TKey, TValue>();
        }

        // 
        ///<summary>
        /// Default constructor overload, which creates the internal <see cref="Dictionary{TKey, TValue}"/>
        /// and sets the capacity.
        ///</summary>
        ///<param name="count"></param>
        public ThreadSafeDictionary(int count)
        {
            _dict = new Dictionary<TKey, TValue>(count);
        }

        #region SpecialThread methods

        // 11/10/2009 - 
        /// <summary>
        /// Checks for the key and add in same atomic operation.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <param name="value">Value to add</param>
        public void SafeAdd(TKey key, TValue value)
        {
            lock (_dictionaryLock)
            {
                // check if key exists first.
                if (!_dict.ContainsKey(key))
                {
                    // add new item.
                    _dict.Add(key, value);
                }
            }
        }

        // 12/18/2009
        /// <summary>
        /// Checks for re-sizing of the given collection, and then perform the copy 
        /// of the Keys in the same atomic operation.
        /// </summary>
        /// <param name="array">Collection to copy to</param>
        /// <param name="arrayIndex">Staring collection index position</param>
        /// <param name="count">(OUT) Number of items copied to collection</param>
        public void KeysSafeCopyTo(ref TKey[] array, int arrayIndex, out int count)
        {
            lock (_dictionaryLock)
            {
                // get Keys current count
                count = _dict.Keys.Count;

                // Verify array is large enough for copy op.
                if (array.Length < count)
                    Array.Resize(ref array, count);

                _dict.Keys.CopyTo(array, arrayIndex);
                
            }
        }

        #endregion


        #region IThreadSafeDictionary<TKey,TValue> Members

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.
        ///                 </param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        ///                 </exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        ///                 </exception>
        public virtual bool Remove(TKey key)
        {
            lock (_dictionaryLock)
            {
                return _dict.Remove(key);
            }
        }


        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        ///                 </param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        ///                 </exception>
        public virtual bool ContainsKey(TKey key)
        {
            lock (_dictionaryLock)
            {
                return _dict.ContainsKey(key);
            }
        }


        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.
        ///                 </param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.
        ///                 </param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        ///                 </exception>
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            lock (_dictionaryLock)
            {
                return _dict.TryGetValue(key, out value);
            }
        }


        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.
        ///                 </param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        ///                 </exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.
        ///                 </exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        ///                 </exception>
        public virtual TValue this[TKey key]
        {
            get
            {
                lock (_dictionaryLock)
                {
                    return _dict[key];
                }
            }
            set
            {
                lock (_dictionaryLock)
                {
                    _dict[key] = value;
                }
            }
        }


        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public virtual ICollection<TKey> Keys
        {
            get
            {
                lock (_dictionaryLock)
                {
                    return new List<TKey>(_dict.Keys);
                }
            }
        }


        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public virtual ICollection<TValue> Values
        {
            get
            {
                lock (_dictionaryLock)
                {
                    return new List<TValue>(_dict.Values);
                }
            }
        }


        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. 
        ///                 </exception>
        public virtual void Clear()
        {
            lock (_dictionaryLock)
            {
                _dict.Clear();
            }
        }


        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public virtual int Count
        {
            get
            {
                lock (_dictionaryLock)
                {
                    return _dict.Count;
                }
            }
        }


        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        ///                 </param>
        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_dictionaryLock)
            {
                return _dict.Contains(item);
            }
        }


        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        ///                 </param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        ///                 </exception>
        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_dictionaryLock)
            {
                _dict.Add(item);
            }
        }


        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.
        ///                 </param><param name="value">The object to use as the value of the element to add.
        ///                 </param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        ///                 </exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        ///                 </exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        ///                 </exception>
        public virtual void Add(TKey key, TValue value)
        {
            lock (_dictionaryLock)
            {
                _dict.Add(key, value);
            }
        }


        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        ///                 </param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        ///                 </exception>
        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_dictionaryLock)
            {
                return _dict.Remove(item);
            }
        }


        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.
        ///                 </param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.
        ///                 </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.
        ///                 </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.
        ///                 </exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.
        ///                     -or-
        ///                 <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        ///                     -or-
        ///                     The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        ///                     -or-
        ///                     Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
        ///                 </exception>
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_dictionaryLock)
            {
                _dict.CopyTo(array, arrayIndex);
            }
        }


        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public virtual bool IsReadOnly
        {
            get
            {
                lock (_dictionaryLock)
                {
                    return _dict.IsReadOnly;
                }
            }
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotSupportedException(
                "Cannot enumerate a threadsafe dictionary.  Instead, enumerate the keys or values collection");
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException(
                "Cannot enumerate a threadsafe dictionary.  Instead, enumerate the keys or values collection");
        }

        #endregion
    }
}