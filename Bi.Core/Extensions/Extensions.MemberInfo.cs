using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json;
using Bi.Core.Helpers;

namespace Bi.Core.Extensions
{
    /// <summary>
    /// MemberInfo扩展类
    /// </summary>
    public static class MemberInfoExtensions
    {
        #region Attribute
        /// <summary>
        /// 获取首个指定特性
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetFirstOrDefaultAttribute<T>() as T;
        }

        /// <summary>
        /// 获取指定特性集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object[] GetAttributes<T>(this MemberInfo @this) where T : Attribute
        {
            return @this?.GetCustomAttributes(typeof(T), false);
        }

        /// <summary>
        /// 获取首个指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object GetFirstOrDefaultAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetAttributes<T>()?.FirstOrDefault();
        }

        /// <summary>
        /// 是否包含指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool ContainsAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetAttributes<T>()?.Length > 0;
        }
        #endregion

        #region JsonProperty
        /// <summary>
        /// 获取JsonProperty特性名称
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string GetJsonProperty(this MemberInfo @this)
        {
            var result = @this.Name;
            if (@this?.GetFirstOrDefaultAttribute<JsonPropertyAttribute>() is JsonPropertyAttribute jpa)
            {
                result = jpa.PropertyName;
            }
            return result;
        }
        #endregion

        #region Description
        /// <summary>
        /// 获取DescriptionAttribute特性名称
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string GetDescription(this MemberInfo @this)
        {
            var result = @this.Name;
            if (@this?.GetFirstOrDefaultAttribute<DescriptionAttribute>() is DescriptionAttribute da)
            {
                result = da.Description;
            }
            return result;
        }
        #endregion

        #region ExcelColumn
        /// <summary>
        /// 获取ExcelColumnAttribute特性列名称
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string GetExcelColumn(this MemberInfo @this)
        {
            var result = @this.Name;
            if (@this?.GetFirstOrDefaultAttribute<ExcelColumnAttribute>() is ExcelColumnAttribute attribute)
            {
                result = attribute.ColumnName;
            }
            return result;
        }
        #endregion
    }
}
