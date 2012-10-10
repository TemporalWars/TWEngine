using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

// 3/7/2011 - New SortedList, used to give this functionality back to the Xbox, which was removed in XNA 4.0
namespace ImageNexus.BenScharbach.TWLate.Xbox360Generics
{
    //[Serializable, DebuggerTypeProxy(typeof(System_DictionaryDebugView<,>)), DebuggerDisplay("Count = {Count}"), ComVisible(false)]
    public class SortedList<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>,
                                            IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection,
                                            IEnumerable
    {
        // Fields
        private static readonly TKey[] EmptyKeys;
        private static readonly TValue[] EmptyValues;
        private readonly IComparer<TKey> _comparer;
        private KeyList _keyList;
        private TKey[] _keys;
        private int _size;

        //[NonSerialized]
        private object _syncRoot;
        private ValueList _valueList;
        private TValue[] _values;
        private int _version;

        // Methods
        static SortedList()
        {
            EmptyKeys = new TKey[0];
            EmptyValues = new TValue[0];
        }

        public SortedList()
        {
            _keys = EmptyKeys;
            _values = EmptyValues;
            _size = 0;
            _comparer = Comparer<TKey>.Default;
        }

        public SortedList(IComparer<TKey> comparer)
            : this()
        {
            if (comparer != null)
            {
                _comparer = comparer;
            }
        }

        //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SortedList(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }

        public SortedList(int capacity)
        {
            if (capacity < 0)
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNumRequired);
                throw new ArgumentOutOfRangeException("capacity");
            }
            _keys = new TKey[capacity];
            _values = new TValue[capacity];
            _comparer = Comparer<TKey>.Default;
        }

        public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
            : this((dictionary != null) ? dictionary.Count : 0, comparer)
        {
            if (dictionary == null)
            {
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                throw new ArgumentNullException("dictionary");
            }
            dictionary.Keys.CopyTo(_keys, 0);
            dictionary.Values.CopyTo(_values, 0);
            Array.Sort(_keys, _values, comparer);
            _size = dictionary.Count;
        }

        public SortedList(int capacity, IComparer<TKey> comparer)
            : this(comparer)
        {
            Capacity = capacity;
        }

        public int Capacity
        {
            get { return _keys.Length; }
            set
            {
                if (value != _keys.Length)
                {
                    if (value < _size)
                    {
                        //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
                        throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_SmallCapacity");
                    }
                    if (value > 0)
                    {
                        var destinationArray = new TKey[value];
                        var localArray2 = new TValue[value];
                        if (_size > 0)
                        {
                            Array.Copy(_keys, 0, destinationArray, 0, _size);
                            Array.Copy(_values, 0, localArray2, 0, _size);
                        }
                        _keys = destinationArray;
                        _values = localArray2;
                    }
                    else
                    {
                        _keys = EmptyKeys;
                        _values = EmptyValues;
                    }
                }
            }
        }

        public IComparer<TKey> Comparer
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _comparer; }
        }

        public IList<TKey> Keys
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetKeyListHelper(); }
        }

        public IList<TValue> Values
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetValueListHelper(); }
        }

        #region IDictionary Members

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                throw new ArgumentException("Arg_RankMultiDimNotSupported");
            }
            if (array.GetLowerBound(0) != 0)
            {
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                throw new ArgumentException("Arg_NonZeroLowerBound");
            }
            if ((arrayIndex < 0) || (arrayIndex > array.Length))
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                throw new ArgumentOutOfRangeException("arrayIndex", "ArgumentOutOfRange_NeedNonNegNum");
            }
            if ((array.Length - arrayIndex) < Count)
            {
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
            }
            var pairArray = array as KeyValuePair<TKey, TValue>[];
            if (pairArray != null)
            {
                for (int i = 0; i < Count; i++)
                {
                    pairArray[i + arrayIndex] = new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
                }
            }
            else
            {
                var objArray = array as object[];
                if (objArray == null)
                {
                    //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    throw new ArgumentException("Argument_InvalidArrayType");
                }
                try
                {
                    for (int j = 0; j < Count; j++)
                    {
                        objArray[j + arrayIndex] = new KeyValuePair<TKey, TValue>(_keys[j], _values[j]);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    throw new ArgumentException("Argument_InvalidArrayType");
                }
            }
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                throw new ArgumentNullException("key");
            }
            //ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);
            try
            {
                var local = (TKey)key;
                try
                {
                    Add(local, (TValue)value);
                }
                catch (InvalidCastException)
                {
                    //ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                    throw new InvalidOperationException("Wrong Key Type!");
                }
            }
            catch (InvalidCastException)
            {
                //ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
                throw new InvalidOperationException("Wrong Key Type!");
            }
        }

        bool IDictionary.Contains(object key)
        {
            return (IsCompatibleKey(key) && ContainsKey((TKey)key));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, 2);
        }

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
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
                if (IsCompatibleKey(key))
                {
                    int index = IndexOfKey((TKey)key);
                    if (index >= 0)
                    {
                        return _values[index];
                    }
                }
                return null;
            }
            set
            {
                if (!IsCompatibleKey(key))
                {
                    //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                    throw new ArgumentNullException("key");
                }
                //ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);
                try
                {
                    var local = (TKey)key;
                    try
                    {
                        this[local] = (TValue)value;
                    }
                    catch (InvalidCastException)
                    {
                        //ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                        throw new ArgumentOutOfRangeException("key", "Wrong Key Type!");
                    }
                }
                catch (InvalidCastException)
                {
                    //ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
                    throw new ArgumentOutOfRangeException("key", "Wrong Key Type!");
                }
            }
        }

        ICollection IDictionary.Keys
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetKeyListHelper(); }
        }

        ICollection IDictionary.Values
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetValueListHelper(); }
        }

        #endregion

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                throw new ArgumentNullException("key");
            }
            int num = Array.BinarySearch(_keys, 0, _size, key, _comparer);
            if (num >= 0)
            {
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
                throw new ArgumentException("Adding Duplicate!", "key");
            }
            Insert(~num, key, value);
        }

        public void Clear()
        {
            _version++;
            Array.Clear(_keys, 0, _size);
            Array.Clear(_values, 0, _size);
            _size = 0;
        }

        public bool ContainsKey(TKey key)
        {
            return (IndexOfKey(key) >= 0);
        }

        public bool Remove(TKey key)
        {
            int index = IndexOfKey(key);
            if (index >= 0)
            {
                RemoveAt(index);
            }
            return (index >= 0);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);
            return ((index >= 0) && EqualityComparer<TValue>.Default.Equals(_values[index], keyValuePair.Value));
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                throw new ArgumentNullException("array");
            }
            if ((arrayIndex < 0) || (arrayIndex > array.Length))
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                throw new ArgumentOutOfRangeException("arrayIndex", "ArgumentOutOfRange_NeedNonNegNum");
            }
            if ((array.Length - arrayIndex) < Count)
            {
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
            }
            for (int i = 0; i < Count; i++)
            {
                var pair = new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
                array[arrayIndex + i] = pair;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);
            if ((index >= 0) && EqualityComparer<TValue>.Default.Equals(_values[index], keyValuePair.Value))
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, 1);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = IndexOfKey(key);
            if (index >= 0)
            {
                value = _values[index];
                return true;
            }
            value = default(TValue);
            return false;
        }

        // Properties

        public int Count
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _size; }
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = IndexOfKey(key);
                if (index >= 0)
                {
                    return _values[index];
                }
                //ThrowHelper.ThrowKeyNotFoundException();
                throw new KeyNotFoundException();
                return default(TValue);
            }
            set
            {
                if (key == null)
                {
                    //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                    throw new ArgumentNullException("key");
                }
                int index = Array.BinarySearch(_keys, 0, _size, key, _comparer);
                if (index >= 0)
                {
                    _values[index] = value;
                    _version++;
                }
                else
                {
                    Insert(~index, key, value);
                }
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetKeyListHelper(); }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            // [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return GetValueListHelper(); }
        }

        #endregion

        public bool ContainsValue(TValue value)
        {
            return (IndexOfValue(value) >= 0);
        }

        private void EnsureCapacity(int min)
        {
            int num = (_keys.Length == 0) ? 4 : (_keys.Length * 2);
            if (num < min)
            {
                num = min;
            }
            Capacity = num;
        }

        private TValue GetByIndex(int index)
        {
            if ((index < 0) || (index >= _size))
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
                throw new ArgumentOutOfRangeException("index");
            }
            return _values[index];
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this, 1);
        }

        private TKey GetKey(int index)
        {
            if ((index < 0) || (index >= _size))
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
                throw new ArgumentOutOfRangeException("index");
            }
            return _keys[index];
        }

        private KeyList GetKeyListHelper()
        {
            if (_keyList == null)
            {
                _keyList = new KeyList(this);
            }
            return _keyList;
        }

        private ValueList GetValueListHelper()
        {
            if (_valueList == null)
            {
                _valueList = new ValueList(this);
            }
            return _valueList;
        }

        public int IndexOfKey(TKey key)
        {
            if (key == null)
            {
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                throw new ArgumentNullException("key");
            }
            int num = Array.BinarySearch(_keys, 0, _size, key, _comparer);
            if (num < 0)
            {
                return -1;
            }
            return num;
        }

        public int IndexOfValue(TValue value)
        {
            return Array.IndexOf(_values, value, 0, _size);
        }

        private void Insert(int index, TKey key, TValue value)
        {
            if (_size == _keys.Length)
            {
                EnsureCapacity(_size + 1);
            }
            if (index < _size)
            {
                Array.Copy(_keys, index, _keys, index + 1, _size - index);
                Array.Copy(_values, index, _values, index + 1, _size - index);
            }
            _keys[index] = key;
            _values[index] = value;
            _size++;
            _version++;
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                throw new ArgumentNullException("key");
            }
            return (key is TKey);
        }

        public void RemoveAt(int index)
        {
            if ((index < 0) || (index >= _size))
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            }
            _size--;
            if (index < _size)
            {
                Array.Copy(_keys, index + 1, _keys, index, _size - index);
                Array.Copy(_values, index + 1, _values, index, _size - index);
            }
            _keys[_size] = default(TKey);
            _values[_size] = default(TValue);
            _version++;
        }

        public void TrimExcess()
        {
            var num = (int)(_keys.Length * 0.9);
            if (_size < num)
            {
                Capacity = _size;
            }
        }

        // Nested Types
        //[Serializable, StructLayout(LayoutKind.Sequential)]

        #region Nested type: Enumerator

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator,
                                    IEnumerator
        {
            private readonly int _getEnumeratorRetType;
            private readonly SortedList<TKey, TValue> _sortedList;
            private readonly int version;
            private int _index;
            private TKey _key;
            private TValue _value;

            internal Enumerator(SortedList<TKey, TValue> sortedList, int getEnumeratorRetType)
            {
                _sortedList = sortedList;
                _index = 0;
                version = _sortedList._version;
                _getEnumeratorRetType = getEnumeratorRetType;
                _key = default(TKey);
                _value = default(TValue);
            }

            #region IDictionaryEnumerator Members

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if ((_index == 0) || (_index == (_sortedList.Count + 1)))
                    {
                        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    return _key;
                }
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if ((_index == 0) || (_index == (_sortedList.Count + 1)))
                    {
                        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    return new DictionaryEntry(_key, _value);
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if ((_index == 0) || (_index == (_sortedList.Count + 1)))
                    {
                        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    return _value;
                }
            }

            #endregion

            #region IEnumerator<KeyValuePair<TKey,TValue>> Members

            public void Dispose()
            {
                _index = 0;
                _key = default(TKey);
                _value = default(TValue);
            }

            public bool MoveNext()
            {
                if (version != _sortedList._version)
                {
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }
                if (_index < _sortedList.Count)
                {
                    _key = _sortedList._keys[_index];
                    _value = _sortedList._values[_index];
                    _index++;
                    return true;
                }
                _index = _sortedList.Count + 1;
                _key = default(TKey);
                _value = default(TValue);
                return false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get { return new KeyValuePair<TKey, TValue>(_key, _value); }
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((_index == 0) || (_index == (_sortedList.Count + 1)))
                    {
                        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    if (_getEnumeratorRetType == 2)
                    {
                        return new DictionaryEntry(_key, _value);
                    }
                    return new KeyValuePair<TKey, TValue>(_key, _value);
                }
            }

            void IEnumerator.Reset()
            {
                if (version != _sortedList._version)
                {
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }
                _index = 0;
                _key = default(TKey);
                _value = default(TValue);
            }

            #endregion
        }

        #endregion

        //[Serializable, DebuggerTypeProxy(typeof(System_DictionaryKeyCollectionDebugView<,>)), DebuggerDisplay("Count = {Count}")]

        #region Nested type: KeyList

        private sealed class KeyList : IList<TKey>, ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable
        {
            // Fields
            private readonly SortedList<TKey, TValue> _dict;

            // Methods
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal KeyList(SortedList<TKey, TValue> dictionary)
            {
                _dict = dictionary;
            }

            #region ICollection Members

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                if ((array != null) && (array.Rank != 1))
                {
                    //ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                    throw new ArgumentException("Arg_RankMultiDimNotSupported", "array");
                }
                try
                {
                    Array.Copy(_dict._keys, 0, array, arrayIndex, _dict.Count);
                }
                catch (ArrayTypeMismatchException)
                {
                    //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    throw new ArgumentException("Argument_InvalidArrayType", "array");
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_dict).SyncRoot; }
            }

            #endregion

            #region IList<TKey> Members

            public void Add(TKey key)
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
            }

            public void Clear()
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
            }

            public bool Contains(TKey key)
            {
                return _dict.ContainsKey(key);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                Array.Copy(_dict._keys, 0, array, arrayIndex, _dict.Count);
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return new SortedListKeyEnumerator(_dict);
            }

            public int IndexOf(TKey key)
            {
                if (key == null)
                {
                    //ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                    throw new ArgumentNullException("key");
                }
                int num = Array.BinarySearch(_dict._keys, 0, _dict.Count, key, _dict._comparer);
                if (num >= 0)
                {
                    return num;
                }
                return -1;
            }

            public void Insert(int index, TKey value)
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
            }

            public bool Remove(TKey key)
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
                return false;
            }

            public void RemoveAt(int index)
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new SortedListKeyEnumerator(_dict);
            }

            // Properties
            public int Count
            {
                get { return _dict._size; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public TKey this[int index]
            {
                get { return _dict.GetKey(index); }
                set
                {
                    //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
                    throw new NotSupportedException("NotSupported_KeyCollectionSet");
                }
            }

            #endregion
        }

        #endregion

        //[Serializable]

        #region Nested type: SortedListKeyEnumerator

        private sealed class SortedListKeyEnumerator : IEnumerator<TKey>, IDisposable, IEnumerator
        {
            // Fields
            private readonly SortedList<TKey, TValue> _sortedList;
            private readonly int version;
            private TKey _currentKey;
            private int _index;

            // Methods
            internal SortedListKeyEnumerator(SortedList<TKey, TValue> sortedList)
            {
                _sortedList = sortedList;
                version = sortedList._version;
            }

            #region IEnumerator<TKey> Members

            public void Dispose()
            {
                _index = 0;
                _currentKey = default(TKey);
            }

            public bool MoveNext()
            {
                if (version != _sortedList._version)
                {
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }
                if (_index < _sortedList.Count)
                {
                    _currentKey = _sortedList._keys[_index];
                    _index++;
                    return true;
                }
                _index = _sortedList.Count + 1;
                _currentKey = default(TKey);
                return false;
            }

            void IEnumerator.Reset()
            {
                if (version != _sortedList._version)
                {
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }
                _index = 0;
                _currentKey = default(TKey);
            }

            // Properties
            public TKey Current
            {
                //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get { return _currentKey; }
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((_index == 0) || (_index == (_sortedList.Count + 1)))
                    {
                        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    return _currentKey;
                }
            }

            #endregion
        }

        #endregion

        //[Serializable]

        #region Nested type: SortedListValueEnumerator

        private sealed class SortedListValueEnumerator : IEnumerator<TValue>, IDisposable, IEnumerator
        {
            // Fields
            private readonly SortedList<TKey, TValue> _sortedList;
            private readonly int version;
            private TValue _currentValue;
            private int _index;

            // Methods
            internal SortedListValueEnumerator(SortedList<TKey, TValue> sortedList)
            {
                _sortedList = sortedList;
                version = sortedList._version;
            }

            #region IEnumerator<TValue> Members

            public void Dispose()
            {
                _index = 0;
                _currentValue = default(TValue);
            }

            public bool MoveNext()
            {
                if (version != _sortedList._version)
                {
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }
                if (_index < _sortedList.Count)
                {
                    _currentValue = _sortedList._values[_index];
                    _index++;
                    return true;
                }
                _index = _sortedList.Count + 1;
                _currentValue = default(TValue);
                return false;
            }

            void IEnumerator.Reset()
            {
                if (version != _sortedList._version)
                {
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }
                _index = 0;
                _currentValue = default(TValue);
            }

            // Properties
            public TValue Current
            {
                //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get { return _currentValue; }
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((_index == 0) || (_index == (_sortedList.Count + 1)))
                    {
                        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    return _currentValue;
                }
            }

            #endregion
        }

        #endregion

        //[Serializable, DebuggerTypeProxy(typeof(System_DictionaryValueCollectionDebugView<,>)), DebuggerDisplay("Count = {Count}")]

        #region Nested type: ValueList

        private sealed class ValueList : IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, ICollection,
                                         IEnumerable
        {
            // Fields
            private readonly SortedList<TKey, TValue> _dict;

            // Methods
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal ValueList(SortedList<TKey, TValue> dictionary)
            {
                _dict = dictionary;
            }

            #region ICollection Members

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                if ((array != null) && (array.Rank != 1))
                {
                    //ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                    throw new ArgumentException("Arg_RankMultiDimNotSupported", "array");
                }
                try
                {
                    Array.Copy(_dict._values, 0, array, arrayIndex, _dict.Count);
                }
                catch (ArrayTypeMismatchException)
                {
                    //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    throw new ArgumentException("Argument_InvalidArrayType");
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_dict).SyncRoot; }
            }

            #endregion

            #region IList<TValue> Members

            public void Add(TValue key)
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
            }

            public void Clear()
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
            }

            public bool Contains(TValue value)
            {
                return _dict.ContainsValue(value);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                Array.Copy(_dict._values, 0, array, arrayIndex, _dict.Count);
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return new SortedListValueEnumerator(_dict);
            }

            public int IndexOf(TValue value)
            {
                return Array.IndexOf(_dict._values, value, 0, _dict.Count);
            }

            public void Insert(int index, TValue value)
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
            }

            public bool Remove(TValue value)
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
                return false;
            }

            public void RemoveAt(int index)
            {
                //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                throw new NotSupportedException("NotSupported_SortedListNestedWrite");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new SortedListValueEnumerator(_dict);
            }

            // Properties
            public int Count
            {
                get { return _dict._size; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public TValue this[int index]
            {
                get { return _dict.GetByIndex(index); }
                set
                {
                    //ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
                    throw new NotSupportedException("NotSupported_SortedListNestedWrite");
                }
            }

            #endregion
        }

        #endregion
    }
}