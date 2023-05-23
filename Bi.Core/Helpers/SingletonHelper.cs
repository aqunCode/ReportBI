using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Helpers;
/// <summary>
/// 泛型单例工具类
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonHelper<T> where T : class
{
    #region 私有字段
    /// <summary>
    /// 静态私有对象
    /// </summary>
    private static T _instance;

    /// <summary>
    /// 线程对象，线程锁使用
    /// </summary>
    private static readonly object locker = new object();
    #endregion

    #region 公有方法
    /// <summary>
    /// 静态获取实例
    /// </summary>
    /// <returns>T</returns>
    public static T GetInstance()
    {
        if (_instance == null)
        {
            lock (locker)
            {
                if (_instance == null)
                    _instance = Activator.CreateInstance<T>();
            }
        }

        return _instance;
    }

    /// <summary>
    /// 静态获取实例
    /// </summary>
    /// <param name="args">构造参数</param>
    /// <returns>T</returns>
    public static T GetInstance(params object[] args)
    {
        if (_instance == null)
        {
            lock (locker)
            {
                if (_instance == null)
                    _instance = (T)Activator.CreateInstance(typeof(T), args);
            }
        }

        return _instance;
    }
    #endregion
}