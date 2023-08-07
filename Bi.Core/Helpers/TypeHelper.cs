﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Bi.Core.Extensions;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// Type帮助工具类
    /// </summary>
    public static class TypeHelper
    {
        #region CreateInstance
        /// <summary>
        /// 使用区分大小写的搜索，从此程序集中查找指定的类型，然后使用系统激活器创建它的实例。
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeName">要查找类型的 System.Type.FullName。</param>
        /// <returns>返回创建的实例</returns>
        public static object CreateInstance(Assembly assembly, string typeName)
        {
            return assembly.CreateInstance(typeName);
        }

        /// <summary>
        /// 使用可选的区分大小写搜索，从此程序集中查找指定的类型，然后使用系统激活器创建它的实例。
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeName">要查找类型的 System.Type.FullName。</param>
        /// <param name="ignoreCase">如果为 true，则忽略类型名的大小写；否则，为 false。</param>
        /// <returns>返回创建的实例</returns>
        public static object CreateInstance(Assembly assembly, string typeName, bool ignoreCase)
        {
            return assembly.CreateInstance(typeName, ignoreCase);
        }
        #endregion

        #region GetElementType
        /// <summary>
        /// GetElementType
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <returns></returns>
        public static Type GetElementType(Type enumerableType)
        {
            return GetElementTypes(enumerableType, null)[0];
        }

        /// <summary>
        /// GetElementTypes
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Type[] GetElementTypes(Type enumerableType, ElementTypeFlags flags = ElementTypeFlags.None)
        {
            return GetElementTypes(enumerableType, null, flags);
        }

        /// <summary>
        /// GetElementType
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static Type GetElementType(Type enumerableType, IEnumerable enumerable)
        {
            return GetElementTypes(enumerableType, enumerable)[0];
        }
        #endregion

        #region GetElementTypes
        /// <summary>
        /// GetElementTypes
        /// </summary>
        /// <param name="enumerableType"></param>
        /// <param name="enumerable"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Type[] GetElementTypes(Type enumerableType, IEnumerable enumerable, ElementTypeFlags flags = ElementTypeFlags.None)
        {
            if (enumerableType.HasElementType)
            {
                return new[] { enumerableType.GetElementType() };
            }
            var idictionaryType = enumerableType.GetDictionaryType();
            if (idictionaryType != null && flags.HasFlag(ElementTypeFlags.BreakKeyValuePair))
            {
                return idictionaryType.GetTypeInfo().GenericTypeArguments;
            }
            var ienumerableType = enumerableType.GetIEnumerableType();
            if (ienumerableType != null)
            {
                return ienumerableType.GetTypeInfo().GenericTypeArguments;
            }
            if (typeof(IEnumerable).IsAssignableFrom(enumerableType))
            {
                var first = enumerable?.Cast<object>().FirstOrDefault();

                return new[] { first?.GetType() ?? typeof(object) };
            }
            throw new ArgumentException($"Unable to find the element type for type '{enumerableType}'.", nameof(enumerableType));
        }
        #endregion

        #region GetEnumerationType
        /// <summary>
        /// GetEnumerationType
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Type GetEnumerationType(Type enumType)
        {
            if (enumType.IsNullableType())
            {
                enumType = enumType.GetTypeInfo().GenericTypeArguments[0];
            }
            if (!enumType.IsEnum())
            {
                return null;
            }
            return enumType;
        }
        #endregion

        #region GetClassAndInheritInterfaces
        /// <summary>  
        /// 获取程序集中的实现类对应的多个接口
        /// </summary>  
        /// <param name="assemblyName">程序集</param>
        public static Dictionary<Type, Type[]> GetClassAndInheritInterfaces(string assemblyName)
        {
            var result = new Dictionary<Type, Type[]>();
            if (!string.IsNullOrEmpty(assemblyName))
            {
                var assembly = Assembly.Load(assemblyName);
                var ts = assembly.GetTypes().ToList();
                foreach (var item in ts.Where(s => !s.IsInterface))
                {
                    var interfaces = item.GetInterfaces();
                    if (item.IsGenericType) continue;
                    if (interfaces?.Length > 0) result.Add(item, interfaces);
                }
            }
            return result;
        }

        /// <summary>  
        /// 获取程序集中的实现类对应的多个接口
        /// </summary>  
        /// <param name="assemblyNames">程序集数组</param>
        public static Dictionary<Type, Type[]> GetClassAndInheritInterfaces(params string[] assemblyNames)
        {
            var result = new Dictionary<Type, Type[]>();
            if (assemblyNames?.Length > 0)
            {
                foreach (var assemblyName in assemblyNames)
                {
                    result = result.Union(GetClassAndInheritInterfaces(assemblyName)).ToDictionary(o => o.Key, o => o.Value);
                }
            }
            return result;
        }
        #endregion
    }

    /// <summary>
    /// 元素类型标识
    /// </summary>
    public enum ElementTypeFlags
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// BreakKeyValuePair
        /// </summary>
        BreakKeyValuePair = 1
    }
}
