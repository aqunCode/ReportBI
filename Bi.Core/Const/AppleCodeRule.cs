using Bi.Core.Extensions;
using OfficeOpenXml.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bi.Core.Const
{
    /// <summary>
    /// 规则
    /// </summary>
    public class CodeRuleConst
    {
        /// <summary>
        /// A客户规则
        /// </summary>
        public static Dictionary<string, int> Apple => new Dictionary<string, int>
        {
            ["0"] = 0,
            ["1"] = 1,
            ["2"] = 2,
            ["3"] = 3,
            ["4"] = 4,
            ["5"] = 5,
            ["6"] = 6,
            ["7"] = 7,
            ["8"] = 8,
            ["9"] = 9,
            ["A"] = 10,
            ["B"] = 11,
            ["C"] = 12,
            ["D"] = 13,
            ["E"] = 14,
            ["F"] = 15,
            ["G"] = 16,
            ["H"] = 17,
            ["J"] = 18,
            ["K"] = 19,
            ["L"] = 20,
            ["M"] = 21,
            ["N"] = 22,
            ["P"] = 23,
            ["Q"] = 24,
            ["R"] = 25,
            ["S"] = 26,
            ["T"] = 27,
            ["U"] = 28,
            ["V"] = 29,
            ["W"] = 30,
            ["X"] = 31,
            ["Y"] = 32,
            ["Z"] = 33
        };

        /// <summary>
        /// 获取A规则CheckSum
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static string AppleCheckSum(string sn)
        {
            if (sn.IsNullOrWhiteSpace())
                throw new ArgumentNullException("sn can not be empty");

            sn = sn.ToUpper();

            if (sn.Contains("O"))
                throw new ArgumentException("sn not allowed contains 'o' or 'O'");

            if (sn.Contains("I"))
                throw new ArgumentException("sn not allowed contains 'i' or 'I'");

            //判断sn字符串中是否有不满足字符
            if (sn.Any(x => !Apple.Keys.Contains(x.ToString())))
                throw new ArgumentException("sn is invalid contains some invalid character");

            var E = 0;//奇数和
            var O = 0;//偶数和
            var dic = Apple;
            var l = sn.Length;
            for (int i = 0; i < l; i++)
            {
                //偶数和
                if ((i + 1) % 2 == 0)
                    O += dic[sn[i].ToString()];
                //奇数和
                else
                    E += dic[sn[i].ToString()];
            }

            //偶数和*3+奇数和
            var total = O * 3 + E;

            //判断是否整除
            var remainder = total % 34;
            if (remainder == 0)
                return sn + "0";

            return sn + dic.Where(q => q.Value == (34 - remainder)).Select(q => q.Key).FirstOrDefault();
        }
        /// <summary>
        /// 取前1-2位转化为指定位数
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="baseNum"></param>
        /// <param name="toBase"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static string PartToBase(long sn,int snNum, int toBase)
        {
            if (snNum <= 0)
                return "";
            var baseStr = "1";
			for (int i = 0; i < snNum-1; i++)
			{
                baseStr += "0";
            }
            long baseNum = snNum.ToLong();
            //判断是否整除
            var a = sn % baseNum;
            var b = (sn / baseNum).ToLong().ToBase(toBase,Apple.Keys.Join(""));
            var code = $"{b}{a.ToString().PadLeft(snNum - 1, '0')}";
			if (snNum == 1)
			{
                code = b;
            }
            return code;
        }
        /// <summary>
        /// 获取A规则CheckSum
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static bool AppleValidCheckSum(string sn)
        {
            if (sn.IsNullOrWhiteSpace())
                return false;

            //sn[0..^1]等价于sn.Substring(0,sn.Length-1)
            return sn == AppleCheckSum(sn[0..^1]);
        }
    }
}
