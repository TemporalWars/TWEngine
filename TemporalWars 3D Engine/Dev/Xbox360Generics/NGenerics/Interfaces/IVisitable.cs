using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xbox360Generics.NGenerics.Interfaces
{
    // Note: From NGenerics http://code.google.com/p/ngenerics/.
    [Obsolete("Please use the AcceptVisitor extension method on IEnumerable<T>.")]
    public interface IVisitable<T>
    {
        // Methods
        void Accept(IVisitor<T> visitor);
    }

}
