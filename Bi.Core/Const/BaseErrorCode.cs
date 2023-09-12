using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bi.Core.Const
{
    /// <summary>
    /// bi平台通用错误代码
    /// </summary>
    public class BaseErrorCode
    {
        /// <summary>
        /// 记录Summary 是否经过sql计算
        /// </summary>
        public static Boolean SummaryFlag = false;

        /// <summary>
        /// 请勿重复新增
        /// </summary>
        public static double PleaseDoNotAddAgain = 10031;

        /// <summary>
        /// 编码无效
        /// </summary>
        public const double InvalidEncode = 10042;

        /// <summary>
        /// 数据源链接成功
        /// </summary>
        public const double SourceConnectSuccess = 200;


        /********************以下为Api接口返回错误码************************/
        /// <summary>
        /// 成功
        /// </summary>
        public const double Successful = 200;

        /// <summary>
        /// 失败
        /// </summary>
        public const double Fail = -1;
        //-------------------------------------------账号报错

        /// <summary>
        /// 无效输入账号
        /// </summary>
        public const double Invalid_Account = 10000;
        /// <summary>
        /// 无效密码
        /// </summary>
        public const double Invalid_Password = 10001;
        /// <summary>
        /// 账号密码有误
        /// </summary>
        public const double Wrong_AccountPassword = 10002;
        /// <summary>
        /// 无效输入参数
        /// </summary>
        public const double Invalid_Input = 10003;
        /// <summary>
        /// OA用户信息同步失败
        /// </summary>
        public const double Wrong_Sync = 10004;
        //-------------------------------------------增删改查


        /// <summary>
        /// 错误详情【动态的错误信息,使用此错误码,错误提示消息放在message中】
        /// </summary>
        public const double ErrorDetail = 500;

        


        /// <summary>
        /// 无效的图片类型
        /// </summary>
        public const double Invalid_PictureFileType = 10003;

        /// <summary>
        /// 无效的图片大小
        /// </summary>
        public const double Invalid_PictureFileSize = 10004;

        /// <summary>
        /// Api接口未授权
        /// </summary>
        public const double Api_Unauthorized = 10005;

        /// <summary>
        /// 无效的文件类型
        /// </summary>
        public const double Invalid_FileType = 10003.1;

        /// <summary>
        /// 无效的文件大小
        /// </summary>
        public const double Invalid_FileSize = 10004.1;

        /// <summary>
        /// 文件为空
        /// </summary>
        public const double File_Empty = 10004.2;

        /// <summary>
        /// 静态资源配置未找到
        /// </summary>
        public const double AssetsOptionsNotFound = 10004.3;

        /// <summary>
        /// 模糊查询后缀
        /// </summary>
        public static char Suffix = '∞';
    }

}
