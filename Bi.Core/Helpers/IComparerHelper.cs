using System;
using System.Collections.Generic;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// IComparer泛型比较器工具类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IComparerHelper<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _comparer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="comparer">自定义比较委托</param>
        public IComparerHelper(Func<T, T, int> comparer = null)
        {
            _comparer = comparer;
        }

        /// <summary>
        /// 实现比较方法接口
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <returns>int</returns>
        public int Compare(T x, T y)
        {
            if (_comparer == null)
                return string.Compare(x?.ToString(), y?.ToString());

            return _comparer(x, y);
        }
    }
}
