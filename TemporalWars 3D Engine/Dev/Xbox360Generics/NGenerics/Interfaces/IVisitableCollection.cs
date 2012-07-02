using System;
using System.Collections;
using System.Collections.Generic;

namespace Xbox360Generics.NGenerics.Interfaces
{
    // Note: From NGenerics http://code.google.com/p/ngenerics/.
    [Obsolete("Please use the AcceptVisitor extension method on IEnumerable<T>.")]
    public interface IVisitableCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IVisitable<T>
    {
        // Properties
        bool IsEmpty { get; }
        bool IsFixedSize { get; }
        bool IsFull { get; }
    }

}
