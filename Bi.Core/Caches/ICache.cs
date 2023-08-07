using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bi.Core.Caches
{
    /// <summary>
    /// 缓存接口
    /// </summary>
    public interface ICache : IDisposable
    {
        /// <summary>
        /// reids插入数据的时候key默认前缀
        /// </summary>
        /// <param name="prefix">redis 键前缀</param>
        /// <returns></returns>
        ICache UseKeyPrefix(string prefix);
        /// <summary>
        /// 判断redis key值是否存在
        /// </summary>
        /// <param name="key">key值</param>
        /// <returns></returns>
        Task<bool> KeyExistsAsync(string key);
        /// <summary>
        /// 异步根据key值获取value，根据 <T> 自动反序列化为对象
        /// </summary>
        /// <typeparam name="T">反序列化为对象</typeparam>
        /// <param name="key">key值</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);
        Task<List<T>> GetPatternAsync<T>(string patternKey);
        Task<bool> SetAsync<T>(string key, T item);
        Task<bool> SetAsync<T>(string key, T item, int timeoutSeconds);
        /// <summary>
        /// 根据字符串patternKey值获取对应的包含patternKey值的所有值
        /// </summary>
        /// <param name="patternKey"></param>
        /// <returns></returns>
        Task<string[]> KeysLikeAsync(string patternKey);
        Task<long> RemoveAsync(params string[] keys);
        Task<long> RemovePatternAsync(string patternKey);
        Task<T> HashGetAsync<T>(string key, string field);
        Task<IEnumerable<T>> HashGetAsync<T>(string key);
        Task<bool> HashSetAsync<T>(string key, string field, T item);
        Task<string[]> HashKeysAsync(string key);
        Task<long> HashRemoveAsync(string key, params string[] fields);
        Task<long> HashLengthAsync(string key);
        Task<List<(string field, string value)>> HashScanAsync(string key, string pattern, int count);
        /// <summary>
        /// 将有序集合中指定成员的分值增加给定增量值（如果指定成员不存在，则将其添加到有序集合中，并将其分值设置为给定增量值）。该方法返回操作后该成员的新分值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="member">要增加分值的成员</param>
        /// <param name="increment">增加的分值 默认为1</param>
        /// <returns></returns>
        Task<decimal> ZIncrByAsync(string key, string member, decimal increment = 1);
        /// <summary>
        /// 获取 【按增量增加按键存储的有序集合中成员的score】的member 和 value 值
        /// </summary>
        /// <param name="key">redis存储key</param>
        /// <param name="addPrefix">是否使用前缀</param>
        /// <returns>member 和 value 值</returns>
        Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(string key, bool addPrefix = true);
        Task<decimal> ZScoreAsync(string key, string member);
        /// <summary>
        /// redis根据key值新建一个redis的list / redis根据key值在原对应的list末尾添加一个新值
        /// </summary>
        /// <param name="key">key值</param>
        /// <param name="redisValue">要插入list的值</param>
        /// <returns></returns>
        Task<long> ListRightPushAsync(string key,string redisValue);

        //Task<bool> LockTakeAsync(string name, int timeoutSeconds = 180);
        //Task<bool> DistributedLockAsync(string key, Func<Task> @delegate, int timeoutSeconds = 180);
        //Task<(F result, bool lockTake)> DistributedLockAsync<F>(string key, Func<Task<F>> @delegate, int timeoutSeconds = 180);
        //Task<bool> LockReleaseAsync();
        //bool LockRelease();
    }
}