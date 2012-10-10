using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ImageNexus.BenScharbach.TWLate.Xbox360Generics.NGenerics.Enums;
using ImageNexus.BenScharbach.TWLate.Xbox360Generics.NGenerics.Interfaces;

namespace ImageNexus.BenScharbach.TWLate.Xbox360Generics.NGenerics
{
    // Note: From NGenerics http://code.google.com/p/ngenerics/.
    public class Heap<T> : IVisitableCollection<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IVisitable<T>, IHeap<T>
    {
        // Fields
        private readonly IComparer<T> comparerToUse;
        private readonly List<T> data;
        private readonly HeapType thisType;

        // Methods
        public Heap(HeapType type)
            : this(type, Comparer<T>.Default)
        {
        }

        public Heap(HeapType type, IComparer<T> comparer)
        {
            Guard.ArgumentNotNull(comparer, "comparer");
            if ((type != HeapType.Minimum) && (type != HeapType.Maximum))
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.thisType = type;
            var g__initLocal0 = new List<T>();
            g__initLocal0.Add(default(T));
            this.data = g__initLocal0;
            this.comparerToUse = (type == HeapType.Minimum) ? comparer : new ReverseComparer<T>(comparer);
        }

        public Heap(HeapType type, Comparison<T> comparer)
            : this(type, new ComparisonComparer<T>(comparer))
        {
        }

        public Heap(HeapType type, int capacity)
            : this(type, capacity, Comparer<T>.Default)
        {
        }

        public Heap(HeapType type, int capacity, IComparer<T> comparer)
        {
            Guard.ArgumentNotNull(comparer, "comparer");
            if ((type != HeapType.Minimum) && (type != HeapType.Maximum))
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.thisType = type;
            var g__initLocal1 = new List<T>(capacity);
            g__initLocal1.Add(default(T));
            this.data = g__initLocal1;
            this.comparerToUse = (type == HeapType.Minimum) ? comparer : new ReverseComparer<T>(comparer);
        }

        public Heap(HeapType type, int capacity, Comparison<T> comparer)
            : this(type, capacity, new ComparisonComparer<T>(comparer))
        {
        }

        [Obsolete("Please use the AcceptVisitor extension method on IEnumerable<T>.")]
        public void Accept(IVisitor<T> visitor)
        {
            Guard.ArgumentNotNull(visitor, "visitor");
            for (int i = 1; i < this.data.Count; i++)
            {
                if (visitor.HasCompleted)
                {
                    return;
                }
                visitor.Visit(this.data[i]);
            }
        }

        public void Add(T item)
        {
            this.AddItem(item);
        }

        protected virtual void AddItem(T item)
        {
            this.data.Add(default(T));
            int counter = this.data.Count - 1;
            while ((counter > 1) && (this.comparerToUse.Compare(this.data[counter / 2], item) > 0))
            {
                this.data[counter] = this.data[counter / 2];
                counter /= 2;
            }
            this.data[counter] = item;
        }

        public void Clear()
        {
            this.ClearItems();
        }

        protected virtual void ClearItems()
        {
            this.data.RemoveRange(1, this.data.Count - 1);
        }

        public bool Contains(T item)
        {
            return this.data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Guard.ArgumentNotNull(array, "array");
            if ((array.Length - arrayIndex) < this.Count)
            {
                throw new ArgumentException("Not Enough Space in target array.", "array");
            }
            for (int i = 1; i < this.data.Count; i++)
            {
                array[arrayIndex++] = this.data[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var d__ = new d__2<T>(0);
            d__.__this = (Heap<T>)this;
            return d__;
        }

        public T RemoveRoot()
        {
            if (this.Count == 0)
            {
                throw new InvalidOperationException("Heap is Empty.");
            }
            T minimum = this.data[1];
            this.RemoveRootItem(minimum);
            return minimum;
        }

        protected virtual void RemoveRootItem(T item)
        {
            T last = this.data[this.Count];
            this.data.RemoveAt(this.Count);
            if (this.Count > 0)
            {
                int counter = 1;
                while ((counter * 2) < this.data.Count)
                {
                    int child = counter * 2;
                    if (((child + 1) < this.data.Count) && (this.comparerToUse.Compare(this.data[child + 1], this.data[child]) < 0))
                    {
                        child++;
                    }
                    if (this.comparerToUse.Compare(last, this.data[child]) <= 0)
                    {
                        break;
                    }
                    this.data[counter] = this.data[child];
                    counter = child;
                }
                this.data[counter] = last;
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        // Properties
        public int Count
        {
            get
            {
                return (this.data.Count - 1);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.Count == 0);
            }
        }

        [Obsolete("This property will not be available in future versions.")]
        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        [Obsolete("This property will not be available in future versions.")]
        public bool IsFull
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T Root
        {
            get
            {
                if (this.Count == 0)
                {
                    throw new InvalidOperationException("Heap is empty.");
                }
                return this.data[1];
            }
        }

        public HeapType Type
        {
            get
            {
                return this.thisType;
            }
        }

        // Nested Types
        [CompilerGenerated]
        private sealed class d__2<T> : IEnumerator<T>, IEnumerator, IDisposable
        {
            // Fields
            private int __state;
            private T __current;
            public Heap<T> __this;
            public int __3;

            // Methods
            [DebuggerHidden]
            public d__2(int __state)
            {
                this.__state = __state;
            }

            public bool MoveNext()
            {
                switch (this.__state)
                {
                    case 0:
                        this.__state = -1;
                        this.__3 = 1;
                        break;

                    case 1:
                        this.__state = -1;
                        this.__3++;
                        break;

                    default:
                        goto Label_0079;
                }
                if (this.__3 < this.__this.data.Count)
                {
                    this.__current = this.__this.data[this.__3];
                    this.__state = 1;
                    return true;
                }
            Label_0079:
                return false;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            // Properties
            T IEnumerator<T>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.__current;
                }
            }
        }
    }
}
    