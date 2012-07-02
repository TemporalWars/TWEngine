using System;
using System.Collections.Generic;

namespace Xbox360Generics.NGenerics
{
    public sealed class ComparisonComparer<T> : IComparer<T>
    {
        // Fields
        private Comparison<T> comparison;

        // Methods
        public ComparisonComparer(Comparison<T> comparison)
        {
            Guard.ArgumentNotNull(comparison, "comparison");
            this.comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return this.comparison(x, y);
        }

        // Properties
        public Comparison<T> Comparison
        {
            get
            {
                return this.comparison;
            }
            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.comparison = value;
            }
        }
    }

}
