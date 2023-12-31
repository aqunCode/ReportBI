﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Bi.Core.Extensions
{
    /// <summary>
    /// Linq扩展类
    /// </summary>
    public static class LinqExtensions
    {
        #region True
        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() => p => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> True<T1, T2>() => (p1, p2) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> True<T1, T2, T3>() => (p1, p2, p3) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> True<T1, T2, T3, T4>() => (p1, p2, p3, p4) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> True<T1, T2, T3, T4, T5>() => (p1, p2, p3, p4, p5) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> True<T1, T2, T3, T4, T5, T6>() => (p1, p2, p3, p4, p5, p6) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> True<T1, T2, T3, T4, T5, T6, T7>() => (p1, p2, p3, p4, p5, p6, p7) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> True<T1, T2, T3, T4, T5, T6, T7, T8>() => (p1, p2, p3, p4, p5, p6, p7, p8) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> True<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => (p1, p2, p3, p4, p5, p6, p7, p8, p9) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> True<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() => (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) => true;
        #endregion

        #region False
        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() => p => false;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> False<T1, T2>() => (p1, p2) => false;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> False<T1, T2, T3>() => (p1, p2, p3) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> False<T1, T2, T3, T4>() => (p1, p2, p3, p4) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> False<T1, T2, T3, T4, T5>() => (p1, p2, p3, p4, p5) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> False<T1, T2, T3, T4, T5, T6>() => (p1, p2, p3, p4, p5, p6) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> False<T1, T2, T3, T4, T5, T6, T7>() => (p1, p2, p3, p4, p5, p6, p7) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> False<T1, T2, T3, T4, T5, T6, T7, T8>() => (p1, p2, p3, p4, p5, p6, p7, p8) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> False<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => (p1, p2, p3, p4, p5, p6, p7, p8, p9) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> False<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() => (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) => true;
        #endregion

        #region Or
        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> Or<T1, T2>(this Expression<Func<T1, T2, bool>> @this, Expression<Func<T1, T2, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> Or<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> @this, Expression<Func<T1, T2, T3, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> Or<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> @this, Expression<Func<T1, T2, T3, T4, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> Or<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> Or<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> Or<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> Or<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> Or<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> Or<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region And
        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> And<T1, T2>(this Expression<Func<T1, T2, bool>> @this, Expression<Func<T1, T2, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> And<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> @this, Expression<Func<T1, T2, T3, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> And<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> @this, Expression<Func<T1, T2, T3, T4, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> And<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> And<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> And<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> And<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> And<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> And<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region WhereIf
        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereIf<T>(this Expression<Func<T, bool>> @this, bool condition, Expression<Func<T, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereIf<T>(this Expression<Func<T, bool>> @this, bool condition, Expression<Func<T, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> WhereIf<T1, T2>(this Expression<Func<T1, T2, bool>> @this, bool condition, Expression<Func<T1, T2, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> WhereIf<T1, T2>(this Expression<Func<T1, T2, bool>> @this, bool condition, Expression<Func<T1, T2, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> WhereIf<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> @this, bool condition, Expression<Func<T1, T2, T3, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> WhereIf<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> @this, bool condition, Expression<Func<T1, T2, T3, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> WhereIf<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> WhereIf<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> WhereIf<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> WhereIf<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> WhereIf<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> WhereIf<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }
        #endregion

        #region ToLambda
        /// <summary>
        /// ToLambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Expression<T> ToLambda<T>(this Expression @this, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<T>(@this, parameters);
        }
        #endregion

        #region ToObject
        /// <summary>
        /// 转换Expression为object
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object ToObject(this Expression @this)
        {
            var type = @this.Type;
            var nodeType = @this.NodeType;

            if (ExpressionType.Constant == nodeType)
                return (@this as ConstantExpression)?.Value;

            if (ExpressionType.Convert == nodeType)
                return (@this as UnaryExpression)?.Operand?.ToObject();

            if (@this is MemberExpression memberExpr && memberExpr.Expression is ConstantExpression constantExpr)
            {
                var constant = constantExpr.Value;
                var memberInfo = memberExpr.Member;

                if (MemberTypes.Property == memberInfo.MemberType)
                {
                    var propertyInfo = constant?.GetType().GetProperty(memberInfo.Name);
                    return propertyInfo?.GetValue(constant, null);
                }

                if (MemberTypes.Field == memberInfo.MemberType)
                {
                    var fieldInfo = constant?.GetType().GetField(memberInfo.Name);
                    return fieldInfo?.GetValue(constant);
                }
            }

            if (typeof(string) == type)
                return @this.ToLambda<Func<string>>().Compile()();

            if (typeof(short) == type)
                return @this.ToLambda<Func<short>>().Compile()();

            if (typeof(short?) == type)
                return @this.ToLambda<Func<short?>>().Compile()();

            if (typeof(int) == type)
                return @this.ToLambda<Func<int>>().Compile()();

            if (typeof(int?) == type)
                return @this.ToLambda<Func<int?>>().Compile()();

            if (typeof(long) == type)
                return @this.ToLambda<Func<long>>().Compile()();

            if (typeof(long?) == type)
                return @this.ToLambda<Func<long?>>().Compile()();

            if (typeof(decimal) == type)
                return @this.ToLambda<Func<decimal>>().Compile()();

            if (typeof(decimal?) == type)
                return @this.ToLambda<Func<decimal?>>().Compile()();

            if (typeof(double) == type)
                return @this.ToLambda<Func<double>>().Compile()();

            if (typeof(double?) == type)
                return @this.ToLambda<Func<double?>>().Compile()();

            if (typeof(float) == type)
                return @this.ToLambda<Func<float>>().Compile()();

            if (typeof(float?) == type)
                return @this.ToLambda<Func<float?>>().Compile()();

            if (typeof(DateTime) == type)
                return @this.ToLambda<Func<DateTime>>().Compile()();

            if (typeof(DateTime?) == type)
                return @this.ToLambda<Func<DateTime?>>().Compile()();

            if (typeof(bool) == type)
                return @this.ToLambda<Func<bool>>().Compile()();

            if (typeof(bool?) == type)
                return @this.ToLambda<Func<bool?>>().Compile()();

            if (typeof(byte) == type)
                return @this.ToLambda<Func<byte>>().Compile()();

            if (typeof(byte?) == type)
                return @this.ToLambda<Func<byte?>>().Compile()();

            if (typeof(char) == type)
                return @this.ToLambda<Func<char>>().Compile()();

            if (typeof(char?) == type)
                return @this.ToLambda<Func<char?>>().Compile()();

            return @this.ToLambda<Func<object>>().Compile()();
        }

        /// <summary>
        /// 转换Expression为object
        /// </summary>
        /// <param name="this"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static T ToObject<T>(this Expression @this, out bool result)
        {
            try
            {
                result = true;
                return (T)@this.ToObject();
            }
            catch
            {
                result = false;
                return default;
            }
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// linq正序排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> @this, string property)
        {
            return @this.BuildIOrderedQueryable<T>(property, "OrderBy");
        }

        /// <summary>
        /// linq倒叙排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> @this, string property)
        {
            return @this.BuildIOrderedQueryable<T>(property, "OrderByDescending");
        }

        /// <summary>
        /// linq正序多列排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> @this, string property)
        {
            return @this.BuildIOrderedQueryable<T>(property, "ThenBy");
        }

        /// <summary>
        /// linq倒序多列排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> @this, string property)
        {
            return @this.BuildIOrderedQueryable<T>(property, "ThenByDescending");
        }

        /// <summary>
        /// 根据属性和排序方法构建IOrderedQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> BuildIOrderedQueryable<T>(this IQueryable<T> @this, string property, string methodName)
        {
            var props = property?.Split('.');
            if (props.IsNullOrEmpty())
                throw new ArgumentException($"'{property}' can not be null or empty");

            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;

            foreach (var prop in props)
            {
                var pi = type.GetProperty(prop);

                if (pi == null)
                    continue;

                expr = Expression.Property(expr, pi);

                type = pi.PropertyType;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable)
                .GetMethods()
                .Single(
                    method => method.Name == methodName &&
                    method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 2 &&
                    method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { @this, lambda });

            return (IOrderedQueryable<T>)result;
        }
        #endregion

        #region Property
        /// <summary>
        /// Property
        /// </summary>
        /// <param name="this"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Expression Property(this Expression @this, string propertyName)
        {
            return Expression.Property(@this, propertyName);
        }
        #endregion

        #region AndAlso
        /// <summary>
        /// AndAlso
        /// </summary>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression AndAlso(this Expression @this, Expression other)
        {
            return Expression.AndAlso(@this, other);
        }
        #endregion

        #region Call
        /// <summary>
        /// Call
        /// </summary>
        /// <param name="this"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Expression Call(this Expression @this, string methodName, params Expression[] arguments)
        {
            return Expression.Call(@this, @this.Type.GetMethod(methodName), arguments);
        }
        #endregion

        #region GreaterThan
        /// <summary>
        /// GreaterThan
        /// </summary>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression GreaterThan(this Expression @this, Expression other)
        {
            return Expression.GreaterThan(@this, other);
        }
        #endregion
    }
}
