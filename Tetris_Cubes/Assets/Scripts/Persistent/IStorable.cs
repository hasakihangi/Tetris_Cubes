public interface IStorable<T>
{
    void Store(T storageData);
    void Retrieve(T storageData);
}
    
