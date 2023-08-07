using System;

namespace Bi.Core.Helpers
{
    /// <summary>
    /// CPK工具类
    /// </summary>
    public class CpkHelper
    {
        /// <summary>
        /// 计算标准偏差
        /// </summary>
        /// <param name="arrData"></param>
        /// <returns></returns>
        public static float StDev(float[] arrData)
        {
            float xSum = 0F;
            float xAvg = 0F;
            float sSum = 0F;
            float tmpStDev = 0F;
            int arrNum = arrData.Length;
            for (int i = 0; i < arrNum; i++)
            { xSum += arrData[i]; }
            xAvg = xSum / arrNum;
            for (int j = 0; j < arrNum; j++)
            {
                sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
            }
            tmpStDev = Convert.ToSingle(Math.Sqrt((sSum / (arrNum - 1))).ToString());
            return tmpStDev;
        }

        /// <summary>
        /// 技术能力指标要求1.0以上
        /// </summary>
        /// <param name="UpperLimit">上限</param>
        /// <param name="LowerLimit">下限</param>
        /// <param name="StDev">标准偏差</param>
        /// <returns></returns>
        public static float Cp(float UpperLimit, float LowerLimit, float StDev)
        {
            float tmpV = 0F;
            tmpV = UpperLimit - LowerLimit;
            return Math.Abs(tmpV / (6 * StDev));
        }

        /// <summary>
        /// 制程能力指标上限
        /// </summary>
        /// <param name="UpperLimit">上限</param>
        /// <param name="Avage">平均值</param>
        /// <param name="StDev">标准偏差</param>
        /// <returns></returns>
        public static float CpkU(float UpperLimit, float Avage, float StDev)
        {
            float tmpV = 0F;
            tmpV = UpperLimit - Avage;
            return tmpV / (3 * StDev);
        }

        /// <summary>
        /// 制程能力指标下限
        /// </summary>
        /// <param name="LowerLimit">下限</param>
        /// <param name="Avage">平均值</param>
        /// <param name="StDev">标准偏差</param>
        /// <returns></returns>
        public static float CpkL(float LowerLimit, float Avage, float StDev)
        {
            float tmpV = 0F;
            tmpV = Avage - LowerLimit;
            return tmpV / (3 * StDev);
        }

        /// <summary>
        /// 制程能力指标要求1.33以上
        /// </summary>
        /// <param name="CpkU">CpkU</param>
        /// <param name="CpkL">CpkL</param>
        /// <returns></returns>
        public static float Cpk(float CpkU, float CpkL)
        {
            return Math.Abs(Math.Min(CpkU, CpkL));
        }
    }
}
