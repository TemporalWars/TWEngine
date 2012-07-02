namespace Xbox360Generics.NGenerics.Interfaces
{
    // Note: From NGenerics http://code.google.com/p/ngenerics/.
    public interface IHeap<T>
    {
        // Methods
        void Add(T item);
        T RemoveRoot();

        // Properties
        T Root { get; }
    }
    
}
