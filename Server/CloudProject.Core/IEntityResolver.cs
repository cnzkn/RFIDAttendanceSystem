namespace CloudProject.Core;

public interface IEntityResolver<T, K>
{
    (K, string) GetProperties(T instance);
    Task<T?> ResolveAsync(K key, string type);
}
