using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace ParallelTasksComponent.LocklessDictionary
{
    // 6/21/2010 - Created own version of system.Func<t,t1>, because causes crash in TFS Build due to 
    //             ambuigity error between 3.5 and 4 .Net libraries; essentially, a Microsoft error.
    public delegate TResult PtFunc<T, TResult>(T arg);
    public delegate TResult PtFunc<T1, T2, TResult>(T1 arg1, T2 arg2);


    /*[Serializable, 
#if !XBOX360
    DebuggerDisplay("Count = {Count}"),
#endif
     ComVisible(false), 
#if !XBOX360

    HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)
#endif
]*/
    public class LocklessDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        // Fields
        private const int DEFAULT_CAPACITY = 31;
        private const int DEFAULT_CONCURRENCY_MULTIPLIER = 4;
        private readonly IEqualityComparer<TKey> _comparer;
#if !XBOX360
        [NonSerialized]
#endif
        private volatile Node<TKey, TValue>[] _buckets;
#if !XBOX360
        [NonSerialized] 
#endif
        private volatile int[] _countPerLock;
#if !XBOX360
        [NonSerialized]
#endif
        private object[] _locks;

        private KeyValuePair<TKey, TValue>[] _serializationArray;
        private int _serializationCapacity;
        private int _serializationConcurrencyLevel;

        // Methods
        public LocklessDictionary()
            : this(DefaultConcurrencyLevel, 0x1f)
        {
        }

        public LocklessDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, EqualityComparer<TKey>.Default)
        {
        }

        public LocklessDictionary(IEqualityComparer<TKey> comparer)
            : this(DefaultConcurrencyLevel, 0x1f, comparer)
        {
        }

        public LocklessDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(DefaultConcurrencyLevel, collection, comparer)
        {
        }

        public LocklessDictionary(int concurrencyLevel, int capacity)
            : this(concurrencyLevel, capacity, EqualityComparer<TKey>.Default)
        {
        }

        public LocklessDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection,
                                  IEqualityComparer<TKey> comparer)
            : this(concurrencyLevel, 0x1f, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            InitializeFromCollection(collection);
        }

        public LocklessDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel", "ConcurrentDictionary_ConcurrencyLevelMustBePositive");
            }
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "ConcurrentDictionary_CapacityMustNotBeNegative");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            if (capacity < concurrencyLevel)
            {
                capacity = concurrencyLevel;
            }

            _locks = new object[concurrencyLevel];
            for (var i = 0; i < _locks.Length; i++)
            {
                _locks[i] = new object();
            }
            _countPerLock = new int[_locks.Length];
            _buckets = new Node<TKey, TValue>[capacity];
            _comparer = comparer;
        }

        private static int DefaultConcurrencyLevel
        {
            get { return (4*Environment.ProcessorCount); }
        }

        public bool IsEmpty
        {
            get
            {
                var locksAcquired = 0;
                try
                {
                    AcquireAllLocks(ref locksAcquired);

                    var length = _countPerLock.Length; // 6/8/2010
                    for (var i = 0; i < length; i++)
                    {
                        if (_countPerLock[i] != null)
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    ReleaseLocks(0, locksAcquired);
                }
                return true;
            }
        }

        #region IDictionary Members

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "ConcurrentDictionary_IndexIsNegative");
            }
            var locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);
                var num2 = 0;

                var length = _locks.Length; // 6/8/2010
                for (var i = 0; i < length; i++)
                {
                    num2 += _countPerLock[i];
                }
                if (((array.Length - num2) < index) || (num2 < 0))
                {
                    throw new ArgumentException("ConcurrentDictionary_ArrayNotLargeEnough");
                }
                var pairArray = array as KeyValuePair<TKey, TValue>[];
                if (pairArray != null)
                {
                    CopyToPairs(pairArray, index);
                }
                else
                {
                    var entryArray = array as DictionaryEntry[];
                    if (entryArray != null)
                    {
                        CopyToEntries(entryArray, index);
                    }
                    else
                    {
                        var objArray = array as object[];
                        if (objArray == null)
                        {
                            throw new ArgumentException("ConcurrentDictionary_ArrayIncorrectType", "array");
                        }
                        CopyToObjects(objArray, index);
                    }
                }
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        void IDictionary.Add(object key, object value)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!(key is TKey))
            {
                throw new ArgumentException("ConcurrentDictionary_TypeOfKeyIncorrect");
            }
            try
            {
                local = (TValue) value;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("ConcurrentDictionary_TypeOfValueIncorrect");
            }
            ((IDictionary<TKey, TValue>) this).Add((TKey) key, local);
        }

        bool IDictionary.Contains(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return ((key is TKey) && ContainsKey((TKey) key));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(this);
        }

        void IDictionary.Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (!(key is TKey)) return;

            TValue local;
            TryRemove((TKey) key, out local);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException("ConcurrentCollection_SyncRoot_NotSupported");
            }
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        object IDictionary.this[object key]
        {
            get
            {
                TValue local;
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if ((key is TKey) && TryGetValue((TKey) key, out local))
                {
                    return local;
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (!(key is TKey))
                {
                    throw new ArgumentException("ConcurrentDictionary_TypeOfKeyIncorrect");
                }
                if (!(value is TValue))
                {
                    throw new ArgumentException("ConcurrentDictionary_TypeOfValueIncorrect");
                }
                this[(TKey) key] = (TValue) value;
            }
        }

        ICollection IDictionary.Keys
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetKeys(); }
        }

        ICollection IDictionary.Values
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetValues(); }
        }

        #endregion

        #region IDictionary<TKey,TValue> Members

        public void Clear()
        {
            var locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);
                _buckets = new Node<TKey, TValue>[31];
                Array.Clear(_countPerLock, 0, _countPerLock.Length);
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        public bool ContainsKey(TKey key)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return TryGetValue(key, out local);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ((IDictionary<TKey, TValue>) this).Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue local;
            if (!TryGetValue(keyValuePair.Key, out local))
            {
                return false;
            }
            return EqualityComparer<TValue>.Default.Equals(local, keyValuePair.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "ConcurrentDictionary_IndexIsNegative");
            }
            var locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);
                var num2 = 0;

                var length = _locks.Length; // 6/8/2010
                for (var i = 0; i < length; i++)
                {
                    num2 += _countPerLock[i];
                }
                if (((array.Length - num2) < index) || (num2 < 0))
                {
                    throw new ArgumentException("ConcurrentDictionary_ArrayNotLargeEnough");
                }
                CopyToPairs(array, index);
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue local;
            if (keyValuePair.Key == null)
            {
                throw new ArgumentNullException("keyValuePair", "ConcurrentDictionary_ItemKeyIsNull");
            }
            return TryRemoveInternal(keyValuePair.Key, out local, true, keyValuePair.Value);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
            {
                throw new ArgumentException("ConcurrentDictionary_KeyAlreadyExisted");
            }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            TValue local;
            return TryRemove(key, out local);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int num;
            int num2;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            var buckets = _buckets;
            GetBucketAndLockNo(_comparer.GetHashCode(key), out num, out num2, buckets.Length);
            var next = buckets[num];
            Thread.MemoryBarrier();
            while (next != null)
            {
                if (_comparer.Equals(next.Key, key))
                {
                    value = next.Value;
                    return true;
                }
                next = next.Next;
            }
            value = default(TValue);
            return false;
        }

        public int Count
        {
            get
            {
                var num = 0;
                var locksAcquired = 0;
                try
                {
                    AcquireAllLocks(ref locksAcquired);

                    var length = _countPerLock.Length; // 6/8/2010
                    for (var i = 0; i < length; i++)
                    {
                        num += _countPerLock[i];
                    }
                }
                finally
                {
                    ReleaseLocks(0, locksAcquired);
                }
                return num;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue local;
                if (!TryGetValue(key, out local))
                {
                    Debugger.Break();
                    throw new KeyNotFoundException();
                    
                }
                return local;
            }
            set
            {
                TValue local;
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TryAddInternal(key, value, true, true, out local);
            }
        }

        public ICollection<TKey> Keys
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetKeys(); }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        public ICollection<TValue> Values
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetValues(); }
        }

        #endregion

        private void AcquireAllLocks(ref int locksAcquired)
        {
            /*if (CDSCollectionETWBCLProvider.Log.IsEnabled())
            {
                CDSCollectionETWBCLProvider.Log.ConcurrentDictionary_AcquiringAllLocks(this.m_buckets.Length);
            }*/
            AcquireLocks(0, _locks.Length, ref locksAcquired);
        }

        private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
        {
            for (var i = fromInclusive; i < toExclusive; i++)
            {
                //const bool lockTaken = true; // Updated to const True.
                try
                {
                    // NOTE: Updated to use just Monitor.Enter(), since 2nd overload is only for .Net 4!
                    // This shouldn't be a problem if only one Thread adds; just can't have multiple adds at same time.
                    //Monitor.Enter(_locks[i], ref lockTaken);
                    Monitor.Enter(_locks[i]);
                }
                finally
                {
                    //if (lockTaken)
                    {
                        locksAcquired++;
                    }
                }
            }
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, PtFunc<TKey, TValue, TValue> updateValueFactory)
        {
            var local = default(TValue);
            TValue local3;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            do
            {
                if (!TryGetValue(key, out local3))
                {
                    TValue local2;
                    if (!TryAddInternal(key, addValue, false, true, out local2))
                    {
                        continue;
                    }
                    return local2;
                }
                local = updateValueFactory(key, local3);
            } while (!TryUpdate(key, local, local3));
            return local;
        }

        public TValue AddOrUpdate(TKey key, PtFunc<TKey, TValue> addValueFactory,
                                  PtFunc<TKey, TValue, TValue> updateValueFactory)
        {
            TValue local;
            TValue local3;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (addValueFactory == null)
            {
                throw new ArgumentNullException("addValueFactory");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            do
            {
                if (!TryGetValue(key, out local3))
                {
                    TValue local2;
                    local = addValueFactory(key);
                    if (!TryAddInternal(key, local, false, true, out local2))
                    {
                        continue;
                    }
                    return local2;
                }
                local = updateValueFactory(key, local3);
            } while (!TryUpdate(key, local, local3));
            return local;
        }

        [Conditional("DEBUG")]
        private void Assert(bool condition)
        {
        }

        private void CopyToEntries(DictionaryEntry[] array, int index)
        {
            var buckets = _buckets;

            var length = buckets.Length; // 6/8/2010
            for (var i = 0; i < length; i++)
            {
                for (var node = buckets[i]; node != null; node = node.Next)
                {
                    array[index] = new DictionaryEntry(node.Key, node.Value);
                    index++;
                }
            }
        }

        private void CopyToObjects(object[] array, int index)
        {
            var buckets = _buckets;

            var length = buckets.Length; // 6/8/2010
            for (var i = 0; i < length; i++)
            {
                for (var node = buckets[i]; node != null; node = node.Next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.Key, node.Value);
                    index++;
                }
            }
        }

        private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index)
        {
            var buckets = _buckets;

            var length = buckets.Length; // 6/8/2010
            for (var i = 0; i < length; i++)
            {
                for (var node = buckets[i]; node != null; node = node.Next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.Key, node.Value);
                    index++;
                }
            }
        }

        private void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount)
        {
            bucketNo = (hashcode & 0x7fffffff)%bucketCount;
            lockNo = bucketNo%_locks.Length;
        }

        // 6/8/2010
        /// <summary>
        /// Populates a given basic array with the internal keys.  If the given
        /// array is too small, this method will automatically increase the size.
        /// </summary>
        /// <param name="keys">Array of <see cref="TKey"/> keys</param>
        /// <param name="keysCount">(OUT) The actual keys count.</param>
        public void GetKeys(ref TKey[] keys, out int keysCount)
        {
            var locksAcquired = 0;
            keysCount = 0; // 6/8/2010
            try
            {
                AcquireAllLocks(ref locksAcquired);
                var length = _buckets.Length; // 6/8/2010

                // check if collection large enough; if not, then increase.
                if (keys.Length < length)
                    Array.Resize(ref keys, length);

                for (var i = 0; i < length; i++)
                {
                    for (var node = _buckets[i]; node != null; node = node.Next)
                    {
                        keys[keysCount] = node.Key;
                        keysCount++; // 6/8/2010
                    }
                }
                
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        // 6/10/2010
        /// <summary>
        /// Populates a given basic array with the internal values.  If the given
        /// array is too small, this method will automatically increase the size.
        /// </summary>
        /// <param name="values">Array of <see cref="TValue"/> items</param>
        /// <param name="valuesCount">(OUT) The actual values count.</param>
        public void GetValues(ref TValue[] values, out int valuesCount)
        {
            var locksAcquired = 0;
            valuesCount = 0; // 6/10/2010
            try
            {
                AcquireAllLocks(ref locksAcquired);
                var length = _buckets.Length; // 6/8/2010

                // check if collection large enough; if not, then increase.
                if (values.Length < length)
                    Array.Resize(ref values, length);
                
                for (var i = 0; i < length; i++)
                {
                    for (var node = _buckets[i]; node != null; node = node.Next)
                    {
                        values[valuesCount] = node.Value;
                        valuesCount++; // 6/10/2010
                    }
                }
                
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        private ReadOnlyCollection<TKey> GetKeys()
        {
            ReadOnlyCollection<TKey> onlys;
            var locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);
                var list = new List<TKey>();

                var length = _buckets.Length; // 6/8/2010
                for (var i = 0; i < length; i++)
                {
                    for (var node = _buckets[i]; node != null; node = node.Next)
                    {
                        list.Add(node.Key);
                    }
                }
                onlys = new ReadOnlyCollection<TKey>(list);
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
            return onlys;
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            TryAddInternal(key, value, false, true, out local);
            return local;
        }

        public TValue GetOrAdd(TKey key, PtFunc<TKey, TValue> valueFactory)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }
            if (!TryGetValue(key, out local))
            {
                TryAddInternal(key, valueFactory(key), false, true, out local);
            }
            return local;
        }

        private ReadOnlyCollection<TValue> GetValues()
        {
            ReadOnlyCollection<TValue> onlys;
            var locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);
                var list = new List<TValue>();

                var length = _buckets.Length; // 6/8/2010
                for (var i = 0; i < length; i++)
                {
                    for (var node = _buckets[i]; node != null; node = node.Next)
                    {
                        list.Add(node.Value);
                    }
                }
                onlys = new ReadOnlyCollection<TValue>(list);
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
            return onlys;
        }

        private void GrowTable(Node<TKey, TValue>[] buckets)
        {
            var locksAcquired = 0;
            try
            {
                AcquireLocks(0, 1, ref locksAcquired);
                if (buckets == _buckets)
                {
                    int num2;
                    try
                    {
                        num2 = (buckets.Length*2) + 1;
                        while ((((num2%3) == 0) || ((num2%5) == 0)) || ((num2%7) == 0))
                        {
                            num2 += 2;
                        }
                    }
                    catch (OverflowException)
                    {
                        return;
                    }
                    var nodeArray = new Node<TKey, TValue>[num2];
                    var numArray = new int[_locks.Length];
                    AcquireLocks(1, _locks.Length, ref locksAcquired);
                    var length = buckets.Length; // 6/8/2010
                    for (var i = 0; i < length; i++)
                    {
                        Node<TKey, TValue> next;
                        for (var node = buckets[i]; node != null; node = next)
                        {
                            int num4;
                            int num5;
                            next = node.Next;
                            GetBucketAndLockNo(node.Hashcode, out num4, out num5, nodeArray.Length);
                            nodeArray[num4] = new Node<TKey, TValue>(node.Key, node.Value, node.Hashcode,
                                                       nodeArray[num4]);
                            numArray[num5]++;
                        }
                    }
                    _buckets = nodeArray;
                    _countPerLock = numArray;
                }
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
        }

        private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (var pair in collection)
            {
                TValue local;
                if (pair.Key == null)
                {
                    throw new ArgumentNullException("collection");
                }
                if (!TryAddInternal(pair.Key, pair.Value, false, false, out local))
                {
                    throw new ArgumentException("ConcurrentDictionary_SourceContainsDuplicateKeys");
                }
            }
        }

#if !XBOX360
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var serializationArray = _serializationArray;
            _buckets = new Node<TKey, TValue>[_serializationCapacity];
            _countPerLock = new int[_serializationConcurrencyLevel];
            _locks = new object[_serializationConcurrencyLevel];

            var length = _locks.Length; // 6/8/2010
            for (var i = 0; i < length; i++)
            {
                _locks[i] = new object();
            }
            InitializeFromCollection(serializationArray);
            _serializationArray = null;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            _serializationArray = ToArray();
            _serializationConcurrencyLevel = _locks.Length;
            _serializationCapacity = _buckets.Length;
        }
#endif
        private void ReleaseLocks(int fromInclusive, int toExclusive)
        {
            for (var i = fromInclusive; i < toExclusive; i++)
            {
                Monitor.Exit(_locks[i]);
            }
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            KeyValuePair<TKey, TValue>[] pairArray2;
            var locksAcquired = 0;
            try
            {
                AcquireAllLocks(ref locksAcquired);
                var num2 = 0;

                var length = _locks.Length; // 6/8/2010
                for (var i = 0; i < length; i++)
                {
                    num2 += _countPerLock[i];
                }
                var array = new KeyValuePair<TKey, TValue>[num2];
                CopyToPairs(array, 0);
                pairArray2 = array;
            }
            finally
            {
                ReleaseLocks(0, locksAcquired);
            }
            return pairArray2;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return TryAddInternal(key, value, false, true, out local);
        }

        private bool TryAddInternal(TKey key, TValue value, bool updateIfExists, bool acquireLock,
                                    out TValue resultingValue)
        {
            int num2;
            int num3;
            var hashCode = _comparer.GetHashCode(key);
            Label_000D:
            var nodeArray = _buckets;
            GetBucketAndLockNo(hashCode, out num2, out num3, nodeArray.Length);
            var flag = false;
            //var lockTaken = false;
            try
            {
                if (acquireLock)
                {
                    //Monitor.Enter(_locks[num3], ref lockTaken);
                    Monitor.Enter(_locks[num3]);
                }
                if (nodeArray != _buckets)
                {
                    goto Label_000D;
                }
                Node<TKey, TValue> node = null;
                for (var node2 = nodeArray[num2]; node2 != null; node2 = node2.Next)
                {
                    if (_comparer.Equals(node2.Key, key))
                    {
                        if (updateIfExists)
                        {
                            var node3 = new Node<TKey, TValue>(node2.Key, value, hashCode, node2.Next);
                            if (node == null)
                            {
                                nodeArray[num2] = node3;
                            }
                            else
                            {
                                node.Next = node3;
                            }
                            resultingValue = value;
                        }
                        else
                        {
                            resultingValue = node2.Value;
                        }
                        return false;
                    }
                    node = node2;
                }
                nodeArray[num2] = new Node<TKey, TValue>(key, value, hashCode, nodeArray[num2]);
                _countPerLock[num3] += 1;
                if (_countPerLock[num3] > (nodeArray.Length/_locks.Length))
                {
                    flag = true;
                }
            }
            finally
            {
                //if (lockTaken)
                {
                    Monitor.Exit(_locks[num3]);
                }
            }
            if (flag)
            {
                GrowTable(nodeArray);
            }
            resultingValue = value;
            return true;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return TryRemoveInternal(key, out value, false, default(TValue));
        }

        private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue)
        {
            int num;
            int num2;
            object obj2 = null;
            Label_0000:
            var nodeArray = _buckets;
            GetBucketAndLockNo(_comparer.GetHashCode(key), out num, out num2, nodeArray.Length);
            //bool lockTaken = false;
            try
            {
                //Monitor.Enter(obj2 = _locks[num2], ref lockTaken);
                Monitor.Enter(obj2 = _locks[num2]);
                if (nodeArray != _buckets)
                {
                    goto Label_0000;
                }
                Node<TKey, TValue> node = null;
                for (var node2 = _buckets[num]; node2 != null; node2 = node2.Next)
                {
                    if (_comparer.Equals(node2.Key, key))
                    {
                        if (matchValue && !EqualityComparer<TValue>.Default.Equals(oldValue, node2.Value))
                        {
                            value = default(TValue);
                            return false;
                        }
                        if (node == null)
                        {
                            _buckets[num] = node2.Next;
                        }
                        else
                        {
                            node.Next = node2.Next;
                        }
                        value = node2.Value;
                        _countPerLock[num2] -= 1;
                        return true;
                    }
                    node = node2;
                }
            }
            finally
            {
                //if (lockTaken)
                {
                    Monitor.Exit(obj2);
                }
            }
            value = default(TValue);
            return false;
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            int num2;
            int num3;
            bool flag2;
            object obj2 = null;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            var hashCode = _comparer.GetHashCode(key);
            IEqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
            Label_0026:
            var nodeArray = _buckets;
            GetBucketAndLockNo(hashCode, out num2, out num3, nodeArray.Length);
            //bool lockTaken = false;
            try
            {
                //Monitor.Enter(obj2 = _locks[num3], ref lockTaken);
                Monitor.Enter(obj2 = _locks[num3]);
                if (nodeArray != _buckets)
                {
                    goto Label_0026;
                }
                Node<TKey, TValue> node = null;
                for (var node2 = nodeArray[num2]; node2 != null; node2 = node2.Next)
                {
                    if (_comparer.Equals(node2.Key, key))
                    {
                        if (!comparer.Equals(node2.Value, comparisonValue))
                        {
                            return false;
                        }
                        var node3 = new Node<TKey, TValue>(node2.Key, newValue, hashCode, node2.Next);
                        if (node == null)
                        {
                            nodeArray[num2] = node3;
                        }
                        else
                        {
                            node.Next = node3;
                        }
                        return true;
                    }
                    node = node2;
                }
                flag2 = false;
            }
            finally
            {
                //if (lockTaken)
                {
                    Monitor.Exit(obj2);
                }
            }
            return flag2;
        }

        // Properties

        // Nested Types

        #region Nested type: DictionaryEnumerator

        private class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator
        {
            // Fields
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            // Methods
            internal DictionaryEnumerator(LocklessDictionary<TKey, TValue> dictionary)
            {
                _enumerator = dictionary.GetEnumerator();
            }

            #region IDictionaryEnumerator Members

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            // Properties
            public object Current
            {
                get { return Entry; }
            }

            public DictionaryEntry Entry
            {
                get { return new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value); }
            }

            public object Key
            {
                get { return _enumerator.Current.Key; }
            }

            public object Value
            {
                get { return _enumerator.Current.Value; }
            }

            #endregion
        }

        #endregion

        #region Nested type: Node

        private class Node<TKey, TValue>
        {
            // Fields
            internal readonly int Hashcode;
            internal readonly TKey Key;
            internal readonly TValue Value;
            internal volatile Node<TKey, TValue> Next;

            // Methods
            internal Node(TKey key, TValue value, int hashcode)
                : this(key, value, hashcode, null)
            {
            }

            internal Node(TKey key, TValue value, int hashcode, Node<TKey, TValue> next)
            {
                Key = key;
                Value = value;
                Next = next;
                Hashcode = hashcode;
            }
        }

        #endregion
    }
}