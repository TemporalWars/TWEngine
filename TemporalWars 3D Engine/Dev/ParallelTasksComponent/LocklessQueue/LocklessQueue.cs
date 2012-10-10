using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace ImageNexus.BenScharbach.TWTools.ParallelTasksComponent.LocklessQueue 
{
    /// <summary>
    /// Similar to the Net 4.0 ConcurrentQueue, this <see cref="ParallelTasksComponent.LocklessQueue"/> provides thread-safe processing, with
    /// the benefit of working on the Xbox-360.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LocklessQueue<T> : IEnumerable<T>, ICollection

    {
        // Fields
#if !XBOX360
        [NonSerialized]
#endif
        private volatile Segment<T> _head;

        private T[] _serializationArray;

#if !XBOX360
        [NonSerialized]
#endif
        private volatile Segment<T> _tail;

        private const int SEGMENT_SIZE = 32;

        // Methods
        public LocklessQueue()
        {

            _head = _tail = new Segment<T>(0);
        }

        public LocklessQueue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            InitializeFromCollection(collection);
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            ToList().CopyTo(array, index);
        }

        public void Enqueue(T item)
        {
            var wait = new SpinWait();
            while (!_tail.TryAppend(item, ref _tail))
            {
                wait.SpinOnce();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ToList().GetEnumerator();
        }

        private void GetHeadTailPositions(out Segment<T> head, out Segment<T> tail, out int headLow, out int tailHigh)
        {
            head = _head;
            tail = _tail;
            headLow = head.Low;
            tailHigh = tail.High;
            var wait = new SpinWait();
            while ((((head != _head) || (tail != _tail)) || ((headLow != head.Low) || (tailHigh != tail.High))) || (head._index > tail._index))
            {
                wait.SpinOnce();
                head = _head;
                tail = _tail;
                headLow = head.Low;
                tailHigh = tail.High;
            }
        }

        private void InitializeFromCollection(IEnumerable<T> collection)
        {
            _head = _tail = new Segment<T>(0);
            var num = 0;
            foreach (var local in collection)
            {
                _tail.UnsafeAdd(local);
                num++;
                if (num < 32) continue;

                _tail = _tail.UnsafeGrow();
                num = 0;
            }
        }

#if !XBOX360
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            InitializeFromCollection(_serializationArray);
            _serializationArray = null;
        }

        [OnDeserialized]
        private void OnSerializing(StreamingContext context)
        {
            _serializationArray = ToArray();
        }
#endif
        /*bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return TryDequeue(out item);
        }*/

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            ((ICollection)ToList()).CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T[] ToArray()
        {
            return ToList().ToArray();
        }

        private List<T> ToList()
        {
            Segment<T> segment;
            Segment<T> segment2;
            int num;
            int num2;
            GetHeadTailPositions(out segment, out segment2, out num, out num2);
            if (segment == segment2)
            {
                return segment.ToList(num, num2);
            }
            var list = new List<T>(segment.ToList(num, 31));
            for (var segment3 = segment.Next; segment3 != segment2; segment3 = segment3.Next)
            {
                list.AddRange(segment3.ToList(0, 31));
            }
            list.AddRange(segment2.ToList(0, num2));
            return list;
        }

        public bool TryDequeue(out T result)
        {
            while (!IsEmpty)
            {
                if (_head.TryRemove(out result, ref _head))
                {
                    return true;
                }
            }
            result = default(T);
            return false;
        }

        public bool TryPeek(out T result)
        {
            while (!IsEmpty)
            {
                if (_head.TryPeek(out result))
                {
                    return true;
                }
            }
            result = default(T);
            return false;
        }

        // Properties
        public int Count
        {
            get
            {
                Segment<T> segment;
                Segment<T> segment2;
                int num;
                int num2;
                GetHeadTailPositions(out segment, out segment2, out num, out num2);
                if (segment == segment2)
                {
                    return ((num2 - num) + 1);
                }
                var num3 = 32 - num;
                num3 += 32 * ((int)(segment2._index - segment._index) - 1);
                return (num3 + (num2 + 1));
            }
        }

        public bool IsEmpty
        {
            get
            {
                var head = _head;
                if (head.IsEmpty)
                {
                    if (head.Next == null)
                    {
                        return true;
                    }
                    var wait = new SpinWait();
                    while (head.IsEmpty)
                    {
                        if (head.Next == null)
                        {
                            return true;
                        }
                        wait.SpinOnce();
                        head = _head;
                    }
                }
                return false;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException("ConcurrentCollection_SyncRoot_NotSupported");
            }
        }




        // 6/7/2010
        private class Segment<T>
        {
            // Fields
            internal volatile T[] _array;
            private volatile int _high;
            internal readonly long _index;
            private volatile int _low;
            private volatile Segment<T> _next;
            private volatile int[] _state;

            // Methods
            internal Segment(long index)
            {
                _array = new T[32];
                _state = new int[32];
                _high = -1;
                _index = index;
            }

            internal void Grow(ref Segment<T> tail)
            {
                var segment = new Segment<T>(_index + 1);
                _next = segment;
                tail = _next;
            }

            internal List<T> ToList(int start, int end)
            {
                var list = new List<T>();
                for (var i = start; i <= end; i++)
                {
                    var wait = new SpinWait();
                    while (_state[i] == null)
                    {
                        wait.SpinOnce();
                    }
                    list.Add(_array[i]);
                }
                return list;
            }

            internal bool TryAppend(T value, ref Segment<T> tail)
            {
                if (_high >= 31)
                {
                    return false;
                }
                var index = 32;
                try
                {
                }
                finally
                {
                    index = Interlocked.Increment(ref _high);
                    if (index <= 31)
                    {
                        _array[index] = value;
                        _state[index] = 1;
                    }
                    if (index == 31)
                    {
                        Grow(ref tail);
                    }
                }
                return (index <= 31);
            }

            internal bool TryPeek(out T result)
            {
                result = default(T);
                var low = Low;
                if (low > High)
                {
                    return false;
                }
                var wait = new SpinWait();
                while (_state[low] == null)
                {
                    wait.SpinOnce();
                }
                result = _array[low];
                return true;
            }

            internal bool TryRemove(out T result, ref Segment<T> head)
            {
                var wait = new SpinWait();
                var low = Low;
                for (var i = High; low <= i; i = High)
                {
                    if (Interlocked.CompareExchange(ref _low, low + 1, low) == low)
                    {
                        var wait2 = new SpinWait();
                        while (_state[low] == null)
                        {
                            wait2.SpinOnce();
                        }
                        result = _array[low];
                        if ((low + 1) >= 32)
                        {
                            wait2 = new SpinWait();
                            while (_next == null)
                            {
                                wait2.SpinOnce();
                            }
                            head = _next;
                        }
                        return true;
                    }
                    wait.SpinOnce();
                    low = Low;
                }
                result = default(T);
                return false;
            }

            internal void UnsafeAdd(T value)
            {
                _high++;
                _array[_high] = value;
                _state[_high] = 1;
            }

            internal Segment<T> UnsafeGrow()
            {
                var segment = new Segment<T>(_index + 1);
                _next = segment;
                return segment;
            }

            // Properties
            internal int High
            {
                get
                {
                    return Math.Min(_high, 31);
                }
            }

            internal bool IsEmpty
            {
                get
                {
                    return (Low > High);
                }
            }

            internal int Low
            {
                get
                {
                    return Math.Min(_low, 32);
                }
            }

            internal Segment<T> Next
            {
                get
                {
                    return _next;
                }
            }
        } //End Segment

    } // End Class
} // End Namespace