namespace Xbox360Generics.NGenerics.Interfaces
{
    // Note: From NGenerics http://code.google.com/p/ngenerics/.
    public interface IVisitor<T>
    {
        // Methods
        void Visit(T obj);

        // Properties
        bool HasCompleted { get; }
    }


}
