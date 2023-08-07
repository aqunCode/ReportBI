﻿using System.Diagnostics;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// Stopwatc工具类
    /// </summary>
    public class StopwatchHelper
    {
        #region 计时器开始
        /// <summary>
        /// 计时器开始
        /// </summary>
        /// <returns>Stopwatch</returns>
        public static Stopwatch TimerStart()
        {
            var watch = new Stopwatch();
            watch.Reset();
            watch.Start();
            return watch;
        }
        #endregion

        #region 计时器结束
        /// <summary>
        /// 计时器结束
        /// </summary>
        /// <param name="watch">Stopwatch</param>
        /// <returns>string</returns>
        public static string TimerEnd(Stopwatch watch)
        {
            watch.Stop();
            return watch.ElapsedMilliseconds.ToString();
        }
        #endregion
    }
}
