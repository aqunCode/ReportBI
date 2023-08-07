using System;
using System.Threading.Tasks;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// 异步工具类
    /// </summary>
    public class TaskHelper
    {
        /// <summary>  
        /// 异步执行同步方法  
        /// </summary>  
        /// <param name="function">无返回值委托</param>  
        /// <param name="callback">回调方法</param>  
        public static async void RunAsync(Action function, Action callback = null)
        {
            await Task.Run(() => function?.Invoke());
            callback?.Invoke();
        }

        /// <summary>  
        /// 异步执行同步方法
        /// </summary>  
        /// <typeparam name="T">泛型类型</typeparam>  
        /// <param name="function">有返回值委托</param>  
        /// <param name="callback">回调方法</param>  
        public static async void RunAsync<T>(Func<T> function, Action<T> callback = null)
        {
            var result = await Task.Run(() => function == null ? default(T) : function());
            callback?.Invoke(result);
        }
    }
}
