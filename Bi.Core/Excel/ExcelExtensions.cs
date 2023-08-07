using Bi.Core.Const;
using Bi.Core.Extensions;
using Bi.Core.Helpers;
using Bi.Core.Models;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bi.Core.Excel
{
    /// <summary>
    /// Excel扩展类
    /// </summary>
    public static class ExcelExtensions
    {
        /// <summary>
        /// 静态资源配置
        /// </summary>
        public static List<AssetsOptions> Options { get; set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ExcelExtensions()
        {
            Options = ConfigHelper.GetOptions<List<AssetsOptions>>("AssetsOptions");
        }

        /// <summary>
        /// 读取Excel导入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="assetsType"></param>
        /// <returns></returns>
        public static (List<T> result, double code) Read<T>(this IFormFile @this, string assetsType = "excel") where T : class, new()
        {
            //读取并校验
            var code = @this.Read(assetsType).code;
            if (code != BaseErrorCode.Successful)
                return (default, code);

            //读取Excel文件流
            var res = ExcelHelper
                        .EPPlusReadExcel<T>(@this.OpenReadStream())
                        .FirstOrDefault()
                        .DefaultIfNull();

            //判断读取数据是否为空
            if (res.IsNullOrEmpty())
                return (default, BaseErrorCode.File_Empty);

            return (res, BaseErrorCode.Successful);
        }

        /// <summary>
        /// 读取并校验静态资源配置
        /// </summary>
        /// <param name="this"></param>
        /// <param name="assetsType"></param>
        /// <returns></returns>
        public static (AssetsOptions option, double code) Read(this IFormFile @this, string assetsType = "excel")
        {
            //检查文件大小
            if (@this.IsNull() || @this.Length == 0)
                return (default, BaseErrorCode.File_Empty);

            //读取静态资源配置
            var option = Options.DefaultIfNull().FirstOrDefault(x => x.AssetsType.EqualIgnoreCase(assetsType));

            //判断配置是否存在
            if (option.IsNull())
                return (default, BaseErrorCode.AssetsOptionsNotFound);

            //判断文件大小是否满足配置要求
            if (@this.Length > option.MaxSize)
                return (default, BaseErrorCode.Invalid_FileSize);

            //提取上传的文件文件后缀
            var suffix = Path.GetExtension(@this.FileName)?.ToLower();

            //允许的文件格式
            var fileTypes = option.FileTypes.Split(',').Select(x => x.ToLower());

            //检查文件格式
            if (suffix.IsNullOrEmpty() || !fileTypes.Contains(suffix))
                return (default, BaseErrorCode.Invalid_FileType);

            return (option, BaseErrorCode.Successful);
        }

        /// <summary>
        /// 读取指定类型静态资源文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="assetsType"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadAsync(this string fileName, string assetsType = "excel", string template = "Template")
        {
            var options = Options.DefaultIfNull().FirstOrDefault(x => x.AssetsType.EqualIgnoreCase(assetsType));
            if (options.IsNull())
                return null;

            var path = Path.Combine(options.FilePath, template, fileName);
            if (!File.Exists(path))
                return null;

            return await File.ReadAllBytesAsync(path);
        }

        #region Excel Validate
        /// <summary>
        /// 数据校验
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static (bool isValid, string result) Validate<T>(this List<T> input) where T : ExcelInput
        {
            return Validate<T>(input as IEnumerable<T>);
        }

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static (bool isValid, string result) Validate<T>(this IEnumerable<T> input) where T : ExcelInput
        {
            List<string> errorMsg = new List<string>();

            foreach (var item in input)
            {
                var res = Validate<T>(item);
                if (res.isValid == false)
                    errorMsg.Add($"[序号:{item.Number}]:{res.result}");
            }
            return (errorMsg.Count == 0, string.Join(';', errorMsg));
        }

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private static (bool isValid, string result) Validate<T>(this T input) where T : ExcelInput
        {
            List<string> errorMsg = new List<string>();

            Type type = typeof(T);

            foreach (var prop in type.GetProperties())
            {
                if (prop.IsDefined(typeof(ValidationAttribute), true))
                {
                    foreach (ValidationAttribute attribute in prop.GetCustomAttributes(typeof(ValidationAttribute), true))
                    {
                        if (!attribute.IsValid(prop.GetValue(input)))
                        {
                            errorMsg.Add($"[{GetExcelColumnName(prop)}] {attribute.FormatErrorMessage(prop.Name)}");
                        }
                    }
                }
            }

            foreach (var field in type.GetFields())
            {
                if (field.IsDefined(typeof(ValidationAttribute), true))
                {
                    foreach (ValidationAttribute attribute in field.GetCustomAttributes(typeof(ValidationAttribute), true))
                    {
                        if (!attribute.IsValid(field.GetValue(input)))
                        {
                            errorMsg.Add($"[{GetExcelColumnName(field)}]{attribute.FormatErrorMessage(field.Name)}");
                        }
                    }
                }
            }

            return (errorMsg.Count == 0, string.Join(';', errorMsg));
        }

        /// <summary>
        /// 获取ColumnName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string GetExcelColumnName<T>(T member) where T : MemberInfo
        {
            var res = member.Name;
            if (member.IsDefined(typeof(ExcelColumnAttribute), true))
            {
                var attribute = (ExcelColumnAttribute)member.GetCustomAttribute(typeof(ExcelColumnAttribute), true);
                res = attribute.ColumnName;
            }
            return res;
        }
        #endregion
    }
}
