namespace Jsonificate
{
    /// <summary>
    /// Provides an interface for deep cloning using JSON.
    /// </summary>
    public interface IJsonCloner
    {
        /// <summary>
        /// Creates a deep clone of the input item.
        /// </summary>
        T Clone<T>(T item);
    }
}
