﻿using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bi.Core.Extensions;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// Polly异常处理工具类
    /// </summary>
    public class PollyHelper
    {
        #region WaitAndRetry    
        #region Sync
        /// <summary>
        /// Polly重试指定次数
        /// </summary>
        /// <typeparam name="T">Exception类型</typeparam>
        /// <param name="action">待执行委托</param>
        /// <param name="actionException">最后抛出异常处理委托</param>
        /// <param name="sleepDurations">延迟时间</param>
        /// <param name="onRetry">重试事件</param>
        public static void WaitAndRetry<T>(Action action, Action<Exception> actionException, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, int, Context> onRetry = null) where T : Exception
        {
            try
            {
                //异常重试事件
                if (onRetry == null)
                {
                    void OnRetry(Exception exception, TimeSpan time, int count, Context context) => LogHelper.Error(exception, $"异常：{exception.Message}，时间：{time}，重试次数：{count}，内容：{context.ToJson()}");
                    onRetry = OnRetry;
                }
                Policy
                    .Handle<T>()
                    .WaitAndRetry(sleepDurations, onRetry)
                    .Execute(action);
            }
            catch (Exception ex)
            {
                if (actionException != null)
                {
                    actionException(ex);
                }
                else
                {
                    LogHelper.Error(ex, "WaitAndRetry");
                }
            }
        }

        /// <summary>
        /// Polly重试指定次数
        /// </summary>
        /// <typeparam name="T">Exception类型</typeparam>
        /// <param name="retryCount">重试次数</param>
        /// <param name="action">待执行委托</param>
        /// <param name="actionException">最后抛出异常处理委托</param>
        /// <param name="sleepDurationProvider">延迟时间委托</param>
        /// <param name="onRetry">重试事件</param>
        public static void WaitAndRetry<T>(int retryCount, Action action, Action<Exception> actionException, Func<int, TimeSpan> sleepDurationProvider = null, Action<Exception, TimeSpan, int, Context> onRetry = null) where T : Exception
        {
            try
            {
                //延迟机制，默认为2s+当前重试次数；
                if (sleepDurationProvider == null)
                {
                    TimeSpan SleepDurationProvider(int retryAttempt) => TimeSpan.FromSeconds(2 + retryAttempt);
                    sleepDurationProvider = SleepDurationProvider;
                }
                //异常重试事件
                if (onRetry == null)
                {
                    void OnRetry(Exception exception, TimeSpan time, int count, Context context) => LogHelper.Error(exception, $"异常：{exception.Message}，时间：{time}，重试次数：{count}，内容：{context.ToJson()}");
                    onRetry = OnRetry;
                }
                Policy
                    .Handle<T>()
                    .WaitAndRetry(retryCount, sleepDurationProvider, onRetry)
                    .Execute(action);
            }
            catch (Exception ex)
            {
                if (actionException != null)
                {
                    actionException(ex);
                }
                else
                {
                    LogHelper.Error(ex, "WaitAndRetry");
                }
            }
        }

        /// <summary>
        /// Polly重试指定次数
        /// </summary>
        /// <typeparam name="TException">Exception类型</typeparam>
        /// <typeparam name="TResult">异常返回值类型</typeparam>
        /// <param name="action">待执行委托</param>
        /// <param name="resultPredicate">返回值委托</param>
        /// <param name="actionException">最后抛出异常处理委托</param>
        /// <param name="sleepDurations">延迟时间</param>
        /// <param name="onRetry">重试事件</param>
        public static TResult WaitAndRetry<TException, TResult>(Func<TResult> action, Func<TResult, bool> resultPredicate, Action<Exception> actionException, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry = null) where TException : Exception
        {
            try
            {
                //异常重试事件
                if (onRetry == null)
                {
                    void OnRetry(DelegateResult<TResult> delegateResult, TimeSpan time, int count, Context context) => LogHelper.Error(delegateResult.Exception, $"异常：{delegateResult.Exception?.Message}，结果：{delegateResult.Result.ToJson()}，时间：{time}，重试次数：{count}，内容：{context.ToJson()}");
                    onRetry = OnRetry;
                }
                return Policy
                    .Handle<TException>()
                    .OrResult(resultPredicate)
                    .WaitAndRetry(sleepDurations, onRetry)
                    .Execute(action);
            }
            catch (Exception ex)
            {
                if (actionException != null)
                {
                    actionException(ex);
                }
                else
                {
                    LogHelper.Error(ex, "WaitAndRetry");
                }
            }

            return default(TResult);
        }

        /// <summary>
        /// Polly重试指定次数
        /// </summary>
        /// <typeparam name="TException">Exception类型</typeparam>
        /// <typeparam name="TResult">异常返回值类型</typeparam>
        /// <param name="retryCount">重试次数</param>
        /// <param name="action">待执行委托</param>
        /// <param name="resultPredicate">返回值委托</param>
        /// <param name="actionException">最后抛出异常处理委托</param>
        /// <param name="sleepDurationProvider">延迟时间委托</param>
        /// <param name="onRetry">重试事件</param>
        public static TResult WaitAndRetry<TException, TResult>(int retryCount, Func<TResult> action, Func<TResult, bool> resultPredicate, Action<Exception> actionException, Func<int, TimeSpan> sleepDurationProvider = null, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry = null) where TException : Exception
        {
            try
            {
                //延迟机制，默认为2s+当前重试次数；
                if (sleepDurationProvider == null)
                {
                    TimeSpan SleepDurationProvider(int retryAttempt) => TimeSpan.FromSeconds(2 + retryAttempt);
                    sleepDurationProvider = SleepDurationProvider;
                }
                //异常重试事件
                if (onRetry == null)
                {
                    void OnRetry(DelegateResult<TResult> delegateResult, TimeSpan time, int count, Context context) => LogHelper.Error(delegateResult.Exception, $"异常：{delegateResult.Exception?.Message}，结果：{delegateResult.Result.ToJson()}，时间：{time}，重试次数：{count}，内容：{context.ToJson()}");
                    onRetry = OnRetry;
                }
                return Policy
                    .Handle<TException>()
                    .OrResult(resultPredicate)
                    .WaitAndRetry(retryCount, sleepDurationProvider, onRetry)
                    .Execute(action);
            }
            catch (Exception ex)
            {
                if (actionException != null)
                {
                    actionException(ex);
                }
                else
                {
                    LogHelper.Error(ex, "WaitAndRetry");
                }
            }

            return default(TResult);
        }
        #endregion

        #region Async
        /// <summary>
        /// Polly重试指定次数
        /// </summary>
        /// <typeparam name="T">Exception类型</typeparam>
        /// <param name="action">待执行委托</param>
        /// <param name="actionException">最后抛出异常处理委托</param>
        /// <param name="sleepDurations">延迟时间</param>
        /// <param name="onRetry">重试事件</param>
        /// <returns>Task</returns>
        public static async Task WaitAndRetryAsync<T>(Func<Task> action, Action<Exception> actionException, IEnumerable<TimeSpan> sleepDurations, Action<Exception, TimeSpan, int, Context> onRetry = null) where T : Exception
        {
            try
            {
                //异常重试事件
                if (onRetry == null)
                {
                    void OnRetry(Exception exception, TimeSpan time, int count, Context context) => LogHelper.Error(exception, $"异常：{exception.Message}，时间：{time}，重试次数：{count}，内容：{context.ToJson()}");
                    onRetry = OnRetry;
                }
                await Policy
                        .Handle<T>()
                        .WaitAndRetryAsync(sleepDurations, onRetry)
                        .ExecuteAsync(action);
            }
            catch (Exception ex)
            {
                if (actionException != null)
                {
                    actionException(ex);
                }
                else
                {
                    LogHelper.Error(ex, "WaitAndRetry");
                }
            }
        }

        /// <summary>
        /// Polly重试指定次数
        /// </summary>
        /// <typeparam name="T">Exception类型</typeparam>
        /// <param name="retryCount">重试次数</param>
        /// <param name="action">待执行委托</param>
        /// <param name="actionException">最后抛出异常处理委托</param>
        /// <param name="sleepDurationProvider">延迟时间委托</param>
        /// <param name="onRetry">重试事件</param>
        /// <returns>Task</returns>
        public static async Task WaitAndRetryAsync<T>(int retryCount, Func<Task> action, Action<Exception> actionException, Func<int, TimeSpan> sleepDurationProvider = null, Action<Exception, TimeSpan, int, Context> onRetry = null) where T : Exception
        {
            try
            {
                //延迟机制，默认为2s+当前重试次数；
                if (sleepDurationProvider == null)
                {
                    TimeSpan SleepDurationProvider(int retryAttempt) => TimeSpan.FromSeconds(2 + retryAttempt);
                    sleepDurationProvider = SleepDurationProvider;
                }
                //异常重试事件
                if (onRetry == null)
                {
                    void OnRetry(Exception exception, TimeSpan time, int count, Context context) => LogHelper.Error(exception, $"异常：{exception.Message}，时间：{time}，重试次数：{count}，内容：{context.ToJson()}");
                    onRetry = OnRetry;
                }
                await Policy
                        .Handle<T>()
                        .WaitAndRetryAsync(retryCount, sleepDurationProvider, onRetry)
                        .ExecuteAsync(action);
            }
            catch (Exception ex)
            {
                if (actionException != null)
                {
                    actionException(ex);
                }
                else
                {
                    LogHelper.Error(ex, "WaitAndRetry");
                }
            }
        }

        /// <summary>
        /// Polly重试指定次数
        /// </summary>
        /// <typeparam name="TException">Exception类型</typeparam>
        /// <typeparam name="TResult">异常返回值类型</typeparam>
        /// <param name="action">待执行委托</param>
        /// <param name="resultPredicate">返回值委托</param>
        /// <param name="actionException">最后抛出异常处理委托</param>
        /// <param name="sleepDurations">延迟时间</param>
        /// <param name="onRetry">重试事件</param>
        public static async Task<TResult> WaitAndRetryAsync<TException, TResult>(Func<Task<TResult>> action, Func<TResult, bool> resultPredicate, Action<Exception> actionException, IEnumerable<TimeSpan> sleepDurations, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry = null) where TException : Exception
        {
            try
            {
                //异常重试事件
                if (onRetry == null)
                {
                    void OnRetry(DelegateResult<TResult> delegateResult, TimeSpan time, int count, Context context) => LogHelper.Error(delegateResult.Exception, $"异常：{delegateResult.Exception?.Message}，结果：{delegateResult.Result.ToJson()}，时间：{time}，重试次数：{count}，内容：{context.ToJson()}");
                    onRetry = OnRetry;
                }
                return await Policy
                        .Handle<TException>()
                        .OrResult(resultPredicate)
                        .WaitAndRetryAsync(sleepDurations, onRetry)
                        .ExecuteAsync(action);
            }
            catch (Exception ex)
            {
                if (actionException != null)
                {
                    actionException(ex);
                }
                else
                {
                    LogHelper.Error(ex, "WaitAndRetry");
                }
            }

            return default(TResult);
        }

        /// <summary>
        /// Polly重试指定次数
        /// </summary>
        /// <typeparam name="TException">Exception类型</typeparam>
        /// <typeparam name="TResult">异常返回值类型</typeparam>
        /// <param name="retryCount">重试次数</param>
        /// <param name="action">待执行委托</param>
        /// <param name="resultPredicate">返回值委托</param>
        /// <param name="actionException">最后抛出异常处理委托</param>
        /// <param name="sleepDurationProvider">延迟时间委托</param>
        /// <param name="onRetry">重试事件</param>
        public static async Task<TResult> WaitAndRetryAsync<TException, TResult>(int retryCount, Func<Task<TResult>> action, Func<TResult, bool> resultPredicate, Action<Exception> actionException, Func<int, TimeSpan> sleepDurationProvider = null, Action<DelegateResult<TResult>, TimeSpan, int, Context> onRetry = null) where TException : Exception
        {
            try
            {
                //延迟机制，默认为2s+当前重试次数；
                if (sleepDurationProvider == null)
                {
                    TimeSpan SleepDurationProvider(int retryAttempt) => TimeSpan.FromSeconds(2 + retryAttempt);
                    sleepDurationProvider = SleepDurationProvider;
                }
                //异常重试事件
                if (onRetry == null)
                {
                    void OnRetry(DelegateResult<TResult> delegateResult, TimeSpan time, int count, Context context) => LogHelper.Error(delegateResult.Exception, $"异常：{delegateResult.Exception?.Message}，结果：{delegateResult.Result.ToJson()}，时间：{time}，重试次数：{count}，内容：{context.ToJson()}");
                    onRetry = OnRetry;
                }
                return await Policy
                        .Handle<TException>()
                        .OrResult(resultPredicate)
                        .WaitAndRetryAsync(retryCount, sleepDurationProvider, onRetry)
                        .ExecuteAsync(action);
            }
            catch (Exception ex)
            {
                if (actionException != null)
                {
                    actionException(ex);
                }
                else
                {
                    LogHelper.Error(ex, "WaitAndRetry");
                }
            }

            return default(TResult);
        }
        #endregion
        #endregion

        #region WaitAndRetryForever
        #region Sync
        /// <summary>
        /// Polly永久重试机制
        /// </summary>
        /// <typeparam name="T">Exception类型</typeparam>
        /// <param name="action">待执行委托</param>
        /// <param name="sleepDurationProvider">延迟时间委托</param>
        /// <param name="onRetry">重试事件</param>
        public static void WaitAndRetryForever<T>(Action action, Func<int, Exception, Context, TimeSpan> sleepDurationProvider = null, Action<Exception, TimeSpan, Context> onRetry = null) where T : Exception
        {
            //延迟机制，默认为2s+当前重试次数；
            if (sleepDurationProvider == null)
            {
                TimeSpan SleepDurationProvider(int retryAttempt, Exception exception, Context context) => TimeSpan.FromSeconds(2 + retryAttempt);
                sleepDurationProvider = SleepDurationProvider;
            }
            //异常重试事件
            if (onRetry == null)
            {
                void OnRetry(Exception exception, TimeSpan time, Context context) => LogHelper.Error(exception, $"异常：{exception.Message}，时间：{time}，内容：{context.ToJson()}");
                onRetry = OnRetry;
            }
            Policy
               .Handle<T>()
               .WaitAndRetryForever(sleepDurationProvider, onRetry)
               .Execute(action);
        }

        /// <summary>
        /// Polly永久重试机制
        /// </summary>
        /// <typeparam name="TException">Exception类型</typeparam>
        /// <typeparam name="TResult">异常返回值类型</typeparam>
        /// <param name="action">待执行委托</param>
        /// <param name="resultPredicate">返回值委托</param>
        /// <param name="sleepDurationProvider">延迟时间委托</param>
        /// <param name="onRetry">重试事件</param>
        public static TResult WaitAndRetryForever<TException, TResult>(Func<TResult> action, Func<TResult, bool> resultPredicate, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider = null, Action<DelegateResult<TResult>, TimeSpan, Context> onRetry = null) where TException : Exception
        {
            //延迟机制，默认为2s+当前重试次数；
            if (sleepDurationProvider == null)
            {
                TimeSpan SleepDurationProvider(int retryAttempt, DelegateResult<TResult> delegateResult, Context context) => TimeSpan.FromSeconds(2 + retryAttempt);
                sleepDurationProvider = SleepDurationProvider;
            }
            //异常重试事件
            if (onRetry == null)
            {
                void OnRetry(DelegateResult<TResult> delegateResult, TimeSpan time, Context context) => LogHelper.Error(delegateResult.Exception, $"异常：{delegateResult.Exception?.Message}，结果：{delegateResult.Result.ToJson()}，时间：{time}，内容：{context.ToJson()}");
                onRetry = OnRetry;
            }
            return Policy
               .Handle<TException>()
               .OrResult(resultPredicate)
               .WaitAndRetryForever(sleepDurationProvider, onRetry)
               .Execute(action);
        }
        #endregion

        #region Async
        /// <summary>
        /// Polly永久重试机制
        /// </summary>
        /// <typeparam name="T">Exception类型</typeparam>
        /// <param name="action">待执行委托</param>
        /// <param name="sleepDurationProvider">延迟时间委托</param>
        /// <param name="onRetryAsync">重试事件</param>
        /// <returns>Task</returns>
        public static async Task WaitAndRetryForeverAsync<T>(Func<Task> action, Func<int, Exception, Context, TimeSpan> sleepDurationProvider = null, Func<Exception, TimeSpan, Context, Task> onRetryAsync = null) where T : Exception
        {
            //延迟机制，默认为2s+当前重试次数；
            if (sleepDurationProvider == null)
            {
                TimeSpan SleepDurationProvider(int retryAttempt, Exception exception, Context context) => TimeSpan.FromSeconds(2 + retryAttempt);
                sleepDurationProvider = SleepDurationProvider;
            }
            //异常重试事件
            if (onRetryAsync == null)
            {
                Task OnRetryAsync(Exception exception, TimeSpan time, Context context)
                {
                    LogHelper.Error(exception, $"异常：{exception.Message}，时间：{time}，内容：{context.ToJson()}");
                    return Task.FromResult(0);
                }
                onRetryAsync = OnRetryAsync;
            }
            await Policy
                    .Handle<T>()
                    .WaitAndRetryForeverAsync(sleepDurationProvider, onRetryAsync)
                    .ExecuteAsync(action);
        }

        /// <summary>
        /// Polly永久重试机制
        /// </summary>
        /// <typeparam name="TException">Exception类型</typeparam>
        /// <typeparam name="TResult">异常返回值类型</typeparam>
        /// <param name="action">待执行委托</param>
        /// <param name="resultPredicate">返回值委托</param>
        /// <param name="sleepDurationProvider">延迟时间委托</param>
        /// <param name="onRetryAsync">重试事件</param>
        public static async Task<TResult> WaitAndRetryForeverAsync<TException, TResult>(Func<Task<TResult>> action, Func<TResult, bool> resultPredicate, Func<int, DelegateResult<TResult>, Context, TimeSpan> sleepDurationProvider = null, Func<DelegateResult<TResult>, TimeSpan, Context, Task> onRetryAsync = null) where TException : Exception
        {
            //延迟机制，默认为2s+当前重试次数；
            if (sleepDurationProvider == null)
            {
                TimeSpan SleepDurationProvider(int retryAttempt, DelegateResult<TResult> delegateResult, Context context) => TimeSpan.FromSeconds(2 + retryAttempt);
                sleepDurationProvider = SleepDurationProvider;
            }
            //异常重试事件
            if (onRetryAsync == null)
            {
                Task OnRetryAsync(DelegateResult<TResult> delegateResult, TimeSpan time, Context context)
                {
                    LogHelper.Error(delegateResult.Exception, $"异常：{delegateResult.Exception?.Message}，结果：{delegateResult.Result.ToJson()}，时间：{time}，内容：{context.ToJson()}");
                    return Task.FromResult(0);
                }
                onRetryAsync = OnRetryAsync;
            }
            return await Policy
                    .Handle<TException>()
                    .OrResult(resultPredicate)
                    .WaitAndRetryForeverAsync(sleepDurationProvider, onRetryAsync)
                    .ExecuteAsync(action);
        }
        #endregion
        #endregion
    }
}
