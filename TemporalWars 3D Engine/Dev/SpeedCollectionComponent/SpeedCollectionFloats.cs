using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ImageNexus.BenScharbach.TWTools.SpeedCollectionComponent
{
#if !XBOX360
    // 6/3/2010: Optimized by using the InterLocked Thread methods to update values, rather than using locks!
    /// <summary>
    /// Generic <see cref="SpeedCollection"/>, which implements the <see cref="IDictionary"/> interface, but
    /// stores values internally in a simple array.  This should be used in time-sensitive code,
    /// where performance is critical!
    /// </summary>
    /// <remarks>
    /// This should not be used for very large arrays, since the <see cref="SpeedCollection"/> sets the requested
    /// array size in memory twice; one collection for the 'Items', and the 2nd for tracking valid 'Keys'.  By eliminating
    /// the need to micro-manage the memory, like with the <see cref="Dictionary{TKey, TValue}"/> class, the Add, Remove, and
    /// Check routines are FAST!  However, if memory is an issue, then use the regular <see cref="Dictionary{TKey, TValue}"/> class.
    /// </remarks> 
    public class SpeedCollection : IDictionary<int, float>
    {
        // internal array of itemValues
        private float[] _itemValues;
        private int[] _containsKey; // 6/3/2010 - Updated to int, so will work with InterLocked.Exchange() call!
        private int _version; // 8/25/2009

        private readonly List<int> _keys = new List<int>();
        private readonly List<float> _values = new List<float>();

        private readonly List<KeyValuePair<int, float>> _items = new List<KeyValuePair<int, float>>();

#pragma warning disable 169
        ///<summary>
        /// ThreadLock object
        ///</summary>
        public readonly object ThreadLock = new object();
#pragma warning restore 169

        ///<summary>
        /// Creates the <see cref="SpeedCollection{TValue}"/>, using the
        /// given <paramref name="arraySize"/>.
        ///</summary>
        ///<param name="arraySize">Array size to use for new collection</param>
        public SpeedCollection(int arraySize)
        {
            // create empty array
            arraySize += 1; // add one to be safe.
            _itemValues = new float[arraySize];
            _containsKey = new int[arraySize]; // 6/3/2010
        }

        IEnumerator<KeyValuePair<int, float>> IEnumerable<KeyValuePair<int, float>>.GetEnumerator()
        {
            // iterate entire arrays, and add to new list
            _items.Clear();
            lock (ThreadLock)
            {
                var length = _itemValues.Length; // 1/5/2010
                for (var i = 0; i < length; i++)
                {
                    // if key FALSE, continue.
                    if (_containsKey[i] == 0) continue;

                    // create new KeyValuePair item
                    var item = new KeyValuePair<int, float>(i, _itemValues[i]);

                    // add to list
                    _items.Add(item);

                }
            }
            return _items.GetEnumerator();

        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an item to the <see cref="SpeedCollection{TValue}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="SpeedCollection{TValue}"/>.</param>
        public void Add(KeyValuePair<int, float> item)
        {
            // retrieve data from KeyValuePair struct
            var key = item.Key;
            var pathNode = item.Value;

            Add(key, pathNode);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="SpeedCollection{TValue}"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the given <paramref name="key"/> is less than zero.</exception>
        public void Add(int key, float value)
        {
            // make sure key is not outside range of array
            if (key < 0)
                throw new ArgumentOutOfRangeException("key");


            // make sure array is big enough for key
            if (key >= _itemValues.Length)
            {
                lock (ThreadLock)
                {
                    Array.Resize(ref _itemValues, key + 1);
                    Array.Resize(ref _containsKey, key + 1);
                }
            }

            // 6/3/2010 - Updated to use Interlocked call.
            // store into array, using the key as location.
            //_itemValues[key] = value;
            Interlocked.Exchange(ref _itemValues[key], value);

            // 6/3/2010 - Updated to use Interlocked call.
            // Set as _containsKey to value of 1 for TRUE.
            //_containsKey[key] = 1;
            Interlocked.Exchange(ref _containsKey[key], 1); // 6/3/2010


            _version++;
        }

        /// <summary>
        /// Removes all items from the <see cref="SpeedCollection{TValue}"/>.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_itemValues, 0, _itemValues.Length); // 1/6/2010
            Array.Clear(_containsKey, 0, _containsKey.Length);
            _items.Clear(); // 1/6/2010

            _version++;
        }

        /// <summary>
        /// Determines whether the <see cref="SpeedCollection{TValue}"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="SpeedCollection{TValue}"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="SpeedCollection{TValue}"/>.</param>
        public bool Contains(KeyValuePair<int, float> item)
        {
            // retrieve data from KeyValuePair struct
            var key = item.Key;

            return ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="SpeedCollection{TValue}"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="SpeedCollection{TValue}"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="SpeedCollection{TValue}"/>.</param>
        public bool ContainsKey(int key)
        {
            if (key < 0 || key >= _containsKey.Length)
                return false;

            // 6/3/2010 - Updated to 1 for TRUE
            return _containsKey[key] == 1;
        }

        public void CopyTo(KeyValuePair<int, float>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="SpeedCollection{TValue}"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="SpeedCollection{TValue}"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="SpeedCollection{TValue}"/>.</param>
        public bool Remove(KeyValuePair<int, float> item)
        {
            // retrieve data from KeyValuePair struct
            var key = item.Key;

            return Remove(key);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="SpeedCollection{TValue}"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> 
        /// was not found in the original <see cref="SpeedCollection{TValue}"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param>
        public bool Remove(int key)
        {
            // make sure key is not outside range of array
            if (key < 0 || key >= _itemValues.Length)
                return false;

            // 6/3/2010: Updated to use 'InterLocked', and removed Lock call.
            // set as _constainsKey to false.
            //_containsKey[key] = false;
            Interlocked.Exchange(ref _containsKey[key], 0); // 6/3/2010

            _version++;

            return true;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="SpeedCollection{TValue}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="SpeedCollection{TValue}"/>.
        /// </returns>
        public int Count
        {
            get
            {
                var trueCount = 0;

                // 4/21/2010 - The count return should be the number
                //             of 'TRUE' positions in the _containsKey array.
                var length = _containsKey.Length;
                for (var i = 0; i < length; i++)
                {
                    // If set to 'TRUE', then increase counter.
                    if (_containsKey[i] == 1) // 6/3/2010 - Check if 1, for TRUE.
                        trueCount++;
                }

                return trueCount;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="SpeedCollection{TValue}"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, 
        /// if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.
        ///                 </param>
        public bool TryGetValue(int key, out float value)
        {
            // make sure key is not outside range of array
            if (key < 0 || key >= _itemValues.Length)
            {
                value = default(float);
                return false;
            }

            // 6/3/2010: Updated to use InterLocked call.
            // get value first, since something has to be set for 'OUT' param anyway.
            //value = _itemValues[key];
            value = Interlocked.Exchange(ref _itemValues[key], _itemValues[key]); // just want value, so let's just exchange same value!

            // check if key is set, and return result to caller.
            return _containsKey[key] == 1; // 6/3/2010 - Check if 1, for True.

        }


        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="key">The key of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
        public float this[int key]
        {
            get
            {
                // make sure key is not outside range of array
                if (key < 0 || key >= _itemValues.Length)
                    throw new KeyNotFoundException("key");

                // 4/21/2010 - Check if key was set.
                if (_containsKey[key] == 0) // 6/3/2010 - Check if 0, for False.
                    throw new KeyNotFoundException("key");

                // 6/3/2010: Updated to use InterLocked call.
                //return _itemValues[key];
                return Interlocked.Exchange(ref _itemValues[key], _itemValues[key]); // just want value, so let's just exchange same value!

            }
            set { Add(key, value); }
        }


        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the KeyNotFoundException.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements KeyNotFoundException.
        /// </returns>
        public ICollection<int> Keys
        {
            get
            {
                // iterate through internal array, adding all _keys to list
                _keys.Clear();
                lock (ThreadLock)
                {
                    var length = _containsKey.Length; // 4/21/2010
                    for (var i = 0; i < length; i++)
                    {
                        // add 'i', if set to TRUE
                        if (_containsKey[i] == 1) // 6/3/2010 - Check if 1, for True.
                            _keys.Add(i);
                    }
                }

                return _keys;

            }
        }

        // 8/26/2009
        ///<summary>
        /// Returns the internal keys in reverse order.
        ///</summary>
        public ICollection<int> KeysReversed
        {
            get
            {
                // iterate through internal array, adding all _keys to list
                _keys.Clear();
                var length = _containsKey.Length; // 4/21/2010
                for (var i = 0; i < length; i++)
                {
                    // add 'i', if set to TRUE
                    if (_containsKey[i] == 1) // 6/3/2010 - Check if 1, for True.
                        _keys.Add(i);
                }

                // Reverse order
                var num = 0;
                var num2 = (_keys.Count) - 1;

                while (num < num2)
                {
                    var obj2 = _keys[num];
                    _keys[num] = _keys[num2];
                    _keys[num2] = obj2;
                    num++;
                    num2--;
                }

                return _keys;
            }

        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="SpeedCollection{TValue}"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="SpeedCollection{TValue}"/>.
        /// </returns>
        public ICollection<float> Values
        {
            get
            {
                // iterate through internal array, adding all _itemValues to list
                _values.Clear();
                lock (ThreadLock)
                {
                    var length = _containsKey.Length; // 4/21/2010
                    for (var i = 0; i < length; i++)
                    {
                        // add 'i', if set to TRUE
                        if (_containsKey[i] == 1) // 6/3/2010 - Check if 1, for True.
                            _values.Add(_itemValues[i]);
                    }
                }
                return _values;
            }
        }

    } // End Class
#endif
} // End Namespace