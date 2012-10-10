using System;

namespace ImageNexus.BenScharbach.TWLate.Xbox360Generics.NGenerics.Interfaces
{
    // Note: From NGenerics http://code.google.com/p/ngenerics/.
    [Obsolete("Please use the AcceptVisitor extension method on IEnumerable<T>.")]
    public interface IVisitable<T>
    {
        // Methods
        void Accept(IVisitor<T> visitor);
    }

}
