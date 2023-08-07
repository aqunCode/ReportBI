namespace Bi.Core.Interfaces
{
    /// <summary>
    /// 翻译服务接口
    /// </summary>
    public interface ITranslateService : IDependency
    {
        /// <summary>
        /// 翻译错误消息
        /// </summary>
        /// <param name="errorMsg">错误消息，支持 "{0}" -> string.Format格式化的占位符</param>
        /// <param name="args">string.Format格式化的占位符对应参数</param>
        /// <returns></returns>
        string TranslateErrorMsg(string errorMsg, params object[] args);

        /// <summary>
        /// 翻译错误代码
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="args">string.Format格式化的占位符对应参数</param>
        /// <returns></returns>
        string TranslateErrorCode(double errorCode, params object[] args);
    }
}
