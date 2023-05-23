namespace Bi.Core.Caches;
/// <summary>
/// 缓存接口
/// </summary>
public interface ICache : IDisposable
{
    ICache UseKeyPrefix(string prefix);

    Task<T> GetAsync<T>(string key);
    Task<List<T>> GetPatternAsync<T>(string patternKey);
    Task<bool> SetAsync<T>(string key, T item);
    Task<bool> SetAsync<T>(string key, T item, int timeoutSeconds);
    Task<string[]> KeysAsync(string patternKey);
    Task<long> RemoveAsync(params string[] keys);
    Task<long> RemovePatternAsync(string patternKey);
    Task<T> HashGetAsync<T>(string key, string field);
    Task<IEnumerable<T>> HashGetAsync<T>(string key);
    Task<bool> HashSetAsync<T>(string key, string field, T item);
    Task<string[]> HashKeysAsync(string key);
    Task<long> HashRemoveAsync(string key, params string[] fields);
    Task<long> HashLengthAsync(string key);
    Task<List<(string field, string value)>> HashScanAsync(string key, string pattern, int count);
    Task<decimal> ZIncrByAsync(string key, string member, decimal increment = 1);
    Task<decimal> ZScoreAsync(string key, string member);
    //Task<bool> LockTakeAsync(string name, int timeoutSeconds = 180);
    //Task<bool> DistributedLockAsync(string key, Func<Task> @delegate, int timeoutSeconds = 180);
    //Task<(F result, bool lockTake)> DistributedLockAsync<F>(string key, Func<Task<F>> @delegate, int timeoutSeconds = 180);
    //Task<bool> LockReleaseAsync();
    //bool LockRelease();
}