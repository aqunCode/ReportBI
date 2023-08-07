using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// 泛型映射工具类
    /// </summary>
    /// <typeparam name="T">源类型</typeparam>
    /// <typeparam name="F">目标类型</typeparam>
    public static class MapperHelper<T, F>
    {
        /// <summary>
        /// 私有静态字段
        /// </summary>
        private static readonly Func<T, F> map = MapProvider();

        /// <summary>
        /// 私有方法
        /// </summary>
        /// <returns></returns>
        private static Func<T, F> MapProvider()
        {
            var memberBindingList = new List<MemberBinding>();

            var parameterExpression = Expression.Parameter(typeof(T), "x");

            foreach (var item in typeof(F).GetProperties())
            {
                if (!item.CanWrite)
                    continue;

                var propertyInfo = typeof(T).GetProperty(item.Name);

                if (propertyInfo == null)
                    continue;

                var property = Expression.Property(parameterExpression, propertyInfo);

                var memberBinding = Expression.Bind(item, property);

                memberBindingList.Add(memberBinding);
            }

            var memberInitExpression = Expression.MemberInit(
                Expression.New(typeof(F)),
                memberBindingList.ToArray());

            var lambda = Expression.Lambda<Func<T, F>>(
                memberInitExpression,
                new ParameterExpression[]
                {
                    parameterExpression
                });

            return lambda.Compile();
        }

        /// <summary>
        /// 映射方法
        /// </summary>
        /// <param name="entity">待映射的对象</param>
        /// <returns>目标类型对象</returns>
        public static F MapTo(T entity)
        {
            return map(entity);
        }
    }
}
