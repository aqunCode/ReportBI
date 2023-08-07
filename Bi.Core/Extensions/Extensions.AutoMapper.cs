using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AutoMapper;

namespace Bi.Core.Extensions
{
    /// <summary>
    /// AutoMapper扩展类
    /// </summary>
    public static class AutoMapperExtensions
    {
        #region 映射配置缓存
        /// <summary>
        /// 映射配置缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, MapperConfiguration> autoMapperCache = new ConcurrentDictionary<string, MapperConfiguration>();
        #endregion

        #region 获取缓存key
        /// <summary>
        /// 获取缓存key
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destType">目标类型</param>
        /// <returns></returns>
        private static string GetKey(Type sourceType, Type destType)
        {
            return $"{sourceType.FullName}_{destType.FullName}";
        }
        #endregion

        #region 类型映射
        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">源数据</param>
        /// <param name="config">映射配置</param>
        /// <returns>映射后的数据</returns>
        public static T MapTo<T>(this object @this, MapperConfiguration config = null)
        {
            if (@this == null) return default(T);
            if (config == null)
            {
                config = autoMapperCache.GetOrAdd(GetKey(@this.GetType(), typeof(T)),
                    x => new MapperConfiguration(cfg => cfg.CreateMap(@this.GetType(), typeof(T))));
            }
            return config.CreateMapper().Map<T>(@this);
        }

        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="S">源类型</typeparam>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">源数据</param>
        /// <param name="destination">已存在的目标数据</param>
        /// <param name="config">映射配置</param>
        /// <returns>映射后的数据</returns>
        public static T MapTo<S, T>(this S @this, T destination, MapperConfiguration config = null)
            where T : class
            where S : class
        {
            if (@this == null) return default(T);
            if (config == null)
            {
                config = autoMapperCache.GetOrAdd(GetKey(typeof(S), typeof(T)),
                   x => new MapperConfiguration(cfg => cfg.CreateMap<S, T>()));
            }
            return config.CreateMapper().Map(@this, destination);
        }
        #endregion

        #region 集合列表类型映射
        /// <summary>
        ///  集合列表类型映射
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">源数据</param>
        /// <param name="config">映射配置</param>
        /// <returns>映射后的数据</returns>
        public static List<T> MapTo<T>(this IEnumerable @this, MapperConfiguration config = null)
        {
            if (@this == null) return null;
            foreach (var item in @this)
            {
                if (config == null)
                {
                    config = autoMapperCache.GetOrAdd(GetKey(item.GetType(), typeof(T)),
                        x => new MapperConfiguration(cfg => cfg.CreateMap(item.GetType(), typeof(T))));
                }
                break;
            }
            return config?.CreateMapper().Map<List<T>>(@this);
        }

        /// <summary>
        /// 集合列表类型映射
        /// </summary>
        /// <typeparam name="S">源类型</typeparam>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">源数据</param>
        /// <param name="config">映射配置</param>
        /// <returns>映射后的数据</returns>
        public static List<T> MapTo<S, T>(this IEnumerable<S> @this, MapperConfiguration config = null)
        {
            if (@this == null) return null;
            if (config == null)
            {
                config = autoMapperCache.GetOrAdd(GetKey(typeof(S), typeof(T)),
                       x => new MapperConfiguration(cfg => cfg.CreateMap<S, T>()));
            }
            return config.CreateMapper().Map<List<T>>(@this);
        }
        #endregion
    }
}
