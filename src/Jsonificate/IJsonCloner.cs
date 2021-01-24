namespace Jsonificate
{
    public interface IJsonCloner
    {
        T Clone<T>(T item);
    }
}
