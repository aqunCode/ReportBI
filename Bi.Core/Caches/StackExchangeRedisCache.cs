using Bi.Core.Extensions;
using Bi.Core.Helpers;
using StackExchange.Redis.Profiling;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace Bi.Core.Caches;
/// <summary>
/// 注入StackExchangeRedis，要先注入AddStackExchangeRedis
/// </summary>
public class StackExchangeRedisCache : ICache
{
    #region Field
    /// <summary>
    /// redis命令和key提取器
    /// </summary>
    private static readonly Func<object, object> _commandAndKeyFetcher;

    /// <summary>
    /// redis消息提取器
    /// </summary>
    private static readonly Func<object, object> _messageFetcher;

    /// <summary>
    /// ProfiledCommand类型
    /// </summary>
    private static readonly Type _profiledCommandType;

    /// <summary>
    /// 线程锁
    /// </summary>
    private static readonly object _lock = new();


    /// <summary>
    /// 锁名称
    /// </summary>
    private string _lockName;

    /// <summary>
    /// 标记是否已释放
    /// </summary>
    private bool _disposed;
    #endregion

    #region Property
    /// <summary>
    /// Redis客户端
    /// </summary>
    public RedisHelper Redis { get; set; }
    #endregion

    #region Prefix
    private string _prefix;
    /// <summary>
    /// 使用key前缀
    /// </summary>
    /// <param name="use">是否使用前缀，默认：true</param>
    /// <returns></returns>
    public ICache UseKeyPrefix(string prefix)
    {
        _prefix = prefix;
        return this;
    }

    /// <summary>
    /// 添加前缀
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string AddKeyPrefix(string key)
    {
        return _prefix + key;
    }
    #endregion

    #region Constructor
    /// <summary>
    /// 静态构造函数
    /// </summary>
    static StackExchangeRedisCache()
    {
        var messageType = Type.GetType("StackExchange.Redis.Message,StackExchange.Redis", false);
        _profiledCommandType = Type.GetType("StackExchange.Redis.Profiling.ProfiledCommand,StackExchange.Redis", false);

        if (messageType.IsNotNull() && _profiledCommandType.IsNotNull())
        {
            var commandAndKey = messageType.GetProperty("CommandAndKey", BindingFlags.Public | BindingFlags.Instance);
            var messageProperty = _profiledCommandType.GetField("Message", BindingFlags.NonPublic | BindingFlags.Instance);

            if (commandAndKey.IsNotNull() && messageProperty.IsNotNull())
            {
                _messageFetcher = ExpressionHelper.BuildFieldGetter(_profiledCommandType, messageProperty);
                _commandAndKeyFetcher = ExpressionHelper.BuildPropertyGetter(messageType, commandAndKey);
            }
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public StackExchangeRedisCache(RedisHelper redis)
    {
        Redis = redis;
    }
    #endregion

    #region String
    /// <summary>
    /// 获取string缓存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(string key)
    {
        return await Redis.StringGetAsync<T>(AddKeyPrefix(key));
    }

    /// <summary>
    /// 获取string缓存，模糊匹配(不包含前缀)
    /// </summary>
    /// <param name="patternKey"></param>
    /// <returns></returns>
    public async Task<List<T>> GetPatternAsync<T>(string patternKey)
    {
        var res = new List<T>();
        var keys = await KeysAsync(patternKey);
        if (keys.IsNotNullOrEmpty())
        {
            foreach (var key in keys)
            {
                res.AddIf(x => x != null, await GetAsync<T>(AddKeyPrefix(key)));
            }
        }
        return res;
    }

    /// <summary>
    /// 设置无过期时间的string类型缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync<T>(string key, T item)
    {
        return await Redis.StringSetAsync(AddKeyPrefix(key), item);
    }

    /// <summary>
    /// 设置有过期时间的string类型缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <param name="timeoutSeconds"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync<T>(string key, T item, int timeoutSeconds)
    {
        return await Redis.StringSetAsync(AddKeyPrefix(key), item, TimeSpan.FromSeconds(timeoutSeconds));
    }

    /// <summary>
    /// 根据通配符获取查找所有的string类型key(不包含前缀)
    /// </summary>
    /// <param name="patternKey"></param>
    /// <returns></returns>
    public async Task<string[]> KeysAsync(string patternKey)
    {
        return (await Redis.KeysAsync(patternKey, Redis.Database.Database)).ToArray();
    }

    /// <summary>
    /// 移除string缓存
    /// </summary>
    /// <param name="keys"></param>
    public async Task<long> RemoveAsync(params string[] keys)
    {
        var prefixKeys = keys.Select(x => AddKeyPrefix(x)).ToArray();
        return await Redis.KeyDeleteAsync(prefixKeys);
    }

    /// <summary>
    /// 移除string缓存，模糊匹配(不包含前缀)
    /// </summary>
    /// <param name="patternKey"></param>
    /// <returns></returns>
    public async Task<long> RemovePatternAsync(string patternKey)
    {
        var keys = await KeysAsync(patternKey);
        return await RemoveAsync(keys);
    }
    #endregion

    #region Hash
    /// <summary>
    /// 获取hash缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public async Task<T> HashGetAsync<T>(string key, string field)
    {
        return await Redis.HashGetAsync<T>(AddKeyPrefix(key), field);
    }

    /// <summary>
    /// 获取hash缓存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> HashGetAsync<T>(string key)
    {
        return await Redis.HashGetAsync<T>(AddKeyPrefix(key));
    }

    /// <summary>
    /// 设置hash类型缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="field"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<bool> HashSetAsync<T>(string key, string field, T item)
    {
        return await Redis.HashSetAsync(AddKeyPrefix(key), field, item);
    }

    /// <summary>
    /// 获取所有哈希表中的字段
    /// </summary>
    /// <param name="key">不包含prefix</param>
    /// <returns></returns>
    public async Task<string[]> HashKeysAsync(string key)
    {
        return (await Redis.HashKeysAsync(AddKeyPrefix(key))).ToArray();
    }

    /// <summary>
    /// 移除hash缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fields"></param>
    public async Task<long> HashRemoveAsync(string key, params string[] fields)
    {
        if (fields == null || fields.Length == 0)
            fields = await HashKeysAsync(AddKeyPrefix(key));

        return await Redis.HashDeleteAsync(AddKeyPrefix(key), fields);
    }

    /// <summary>
    /// 获取Hash长度
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<long> HashLengthAsync(string key)
    {
        return await Redis.HashLengthAsync(AddKeyPrefix(key));
    }

    /// <summary>
    /// 扫描Hash
    /// </summary>
    /// <param name="key"></param>
    /// <param name="pattern"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public async Task<List<(string field, string value)>> HashScanAsync(string key, string pattern, int count)
    {
        var res = new List<(string field, string value)>();

        var scanRes = Redis.HashScanAsync(AddKeyPrefix(key), pattern, count, 0);
        await foreach (var item in scanRes)
        {
            res.Add((item.Name, item.Value));
        }
        return res;
    }
    #endregion

    #region Incr
    #region Async
    /// <summary>
    ///   有序集合中对指定成员的分数加上增量 increment
    /// </summary>
    /// <param name="key"></param>
    /// <param name="member"></param>
    /// <param name="increment"></param>
    /// <returns></returns>
    public async Task<decimal> ZIncrByAsync(string key, string member, decimal increment = 1)
    {
        return (decimal)(await Redis.SortedSetIncrementAsync(AddKeyPrefix(key), member, (double)increment));
    }

    /// <summary>
    /// 返回有序集中，成员的分数值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="member"></param>
    /// <returns></returns>
    public async Task<decimal> ZScoreAsync(string key, string member)
    {
        return (decimal)(await Redis.SortedSetScoreAsync(AddKeyPrefix(key), member) ?? 0);
    }
    #endregion
    #endregion

    #region Lock
    /// <summary>
    /// 释放分布式锁
    /// </summary>
    /// <returns></returns>
    public bool LockRelease()
    {
        if (_lockName.IsNotNullOrEmpty())
        {
            var key = _lockName;
            var res = Redis.LockRelease(AddKeyPrefix(key), _lockName);
            _lockName = null;
            return res;
        }
        return false;
    }

    #region Async
    /// <summary>
    /// 开启分布式锁
    /// </summary>
    /// <param name="name">锁名称</param>
    /// <param name="timeoutSeconds">锁超时时长，默认：180s</param>
    /// <returns></returns>
    public async Task<bool> LockTakeAsync(string name, int timeoutSeconds = 180)
    {
        return await Redis.LockTakeAsync(AddKeyPrefix(name), name, TimeSpan.FromSeconds(timeoutSeconds));
    }

    /// <summary>
    /// 释放分布式锁
    /// </summary>
    /// <returns></returns>
    public async Task<bool> LockReleaseAsync()
    {
        if (_lockName.IsNotNullOrEmpty())
        {
            var key = _lockName;
            var res = await Redis.LockReleaseAsync(AddKeyPrefix(key), _lockName);

            _lockName = null;

            return res;
        }

        return false;
    }

    /// <summary>
    /// 分布式锁
    /// </summary>
    /// <param name="key">锁名称</param>
    /// <param name="delegate">锁内自定义委托</param>
    /// <param name="timeoutSeconds">锁超时时长，默认：180s</param>
    /// <returns></returns>
    public async Task<bool> DistributedLockAsync(string key, Func<Task> @delegate, int timeoutSeconds = 180)
    {
        if (await LockTakeAsync(key, timeoutSeconds))
        {
            try
            {
                await @delegate?.Invoke();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await LockReleaseAsync();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 分布式锁
    /// </summary>
    /// <typeparam name="F"></typeparam>
    /// <param name="key">锁名称</param>
    /// <param name="delegate">锁内自定义委托</param>
    /// <param name="timeoutSeconds">锁超时时长，默认：180s</param>
    /// <returns></returns>
    public async Task<(F result, bool lockTake)> DistributedLockAsync<F>(string key, Func<Task<F>> @delegate, int timeoutSeconds = 180)
    {
        if (await LockTakeAsync(key, timeoutSeconds))
        {
            try
            {
                return (await @delegate?.Invoke(), true);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await LockReleaseAsync();
            }
        }

        return (default, false);
    }
    #endregion
    #endregion

    #region Dispose
    /// <summary>
    /// 释放分布式锁
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        //释放分布式锁
        LockRelease();

        _disposed = true;
    }
    #endregion

    #region ProfiledCommand
    /// <summary>
    /// 处理redis命令
    /// </summary>
    /// <param name="profiledCommand"></param>
    /// <returns></returns>
    public static RedisProfilingSession ProcessCommand(IProfiledCommand profiledCommand)
    {
        var (address, port) = profiledCommand.EndPoint switch
        {
            IPEndPoint ipEndPoint => (ipEndPoint.Address?.ToString(), ipEndPoint.Port),
            DnsEndPoint dnsEndPoint => (dnsEndPoint.Host, dnsEndPoint.Port),
            _ => (default, default)
        };

        var command = GetCommandAndKey(profiledCommand);
        if (command.IsNull())
        {
            command = GetCommand(profiledCommand);

            if (profiledCommand.RetransmissionOf != null)
            {
                var retransmissionName = GetCommand(profiledCommand.RetransmissionOf);

                command += $" (Retransmission of {retransmissionName}: {profiledCommand.RetransmissionReason})";
            }
        }

        return new RedisProfilingSession
        {
            Database = profiledCommand.Db.ToString(CultureInfo.InvariantCulture),
            Command = command,
            Address = address,
            Port = port,
            Created = profiledCommand.CommandCreated,
            ElapsedMilliseconds = profiledCommand.ElapsedTime.TotalMilliseconds
        };
    }

    /// <summary>
    /// 获取redis命令
    /// </summary>
    /// <param name="profiledCommand"></param>
    /// <returns></returns>
    public static string GetCommand(IProfiledCommand profiledCommand) =>
        profiledCommand.Command.IsNotNullOrEmpty()
            ? profiledCommand.Command
            : "UNKNOWN";

    /// <summary>
    /// 获取redis命令和key
    /// </summary>
    /// <param name="profiledCommand"></param>
    /// <returns></returns>
    public static string GetCommandAndKey(IProfiledCommand profiledCommand)
    {
        if (profiledCommand.GetType() != _profiledCommandType ||
            _messageFetcher.IsNull() ||
            _commandAndKeyFetcher.IsNull())
            return null;

        var message = _messageFetcher(profiledCommand);
        if (message.IsNull())
            return null;

        return _commandAndKeyFetcher(message) as string;
    }
    #endregion
}