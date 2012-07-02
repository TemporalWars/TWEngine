using System.Collections.Generic;

namespace Xbox360Generics.NGenerics
{
    // Note: From NGenerics http://code.google.com/p/ngenerics/.
    public sealed class ReverseComparer<T> : IComparer<T>
    {
        // Fields
        private IComparer<T> comparerToUse;

        // Methods
        public ReverseComparer()
        {
            this.comparerToUse = Comparer<T>.Default;
        }

        public ReverseComparer(IComparer<T> comparer)
        {
            Guard.ArgumentNotNull(comparer, "comparer");
            this.comparerToUse = comparer;
        }

        public int Compare(T x, T y)
        {
            return this.comparerToUse.Compare(y, x);
        }

        // Properties
        public IComparer<T> Comparer
        {
            get
            {
                return this.comparerToUse;
            }
            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.comparerToUse = value;
            }
        }
    }


}
