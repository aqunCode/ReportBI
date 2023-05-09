using NLog;
using System.Reflection;

namespace Bi.Core.Helpers;
/// <summary>
/// 日志工具类
/// </summary>
public class LogHelper
{
    /// <summary>
    /// Logger
    /// </summary>
    private static readonly Logger logger;

    /// <summary>
    /// Constructor
    /// </summary>
    static LogHelper()
    {
        var nlog = @"XmlConfig/NLog.config".GetFullPath();

        if (!File.Exists(nlog))
        {
            //校验文件夹是否存在，不存在则创建XmlConfig文件夹
            "XmlConfig".GetFullPath().CreateIfNotExists();

            var assembly = Assembly.Load("Baize.Core");
            var manifestResource = "Baize.Core.XmlConfig.NLog.config";

            FileHelper.CreateFileFromManifestResource(assembly, manifestResource, nlog);
        }

        LogManager.LogFactory.LoadConfiguration(nlog);
        logger = LogManager.GetCurrentClassLogger();
    }

    /// <summary>
    /// Debug
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Debug(string message, params object[] args)
    {
        logger.Debug(message, args);
    }

    /// <summary>
    /// Debug
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="message"></param>
    public static void Debug(Exception ex, string message)
    {
        logger.Debug(ex, message);
    }

    /// <summary>
    /// Info
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Info(string message, params object[] args)
    {
        logger.Info(message, args);
    }

    /// <summary>
    /// Info
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="message"></param>
    public static void Info(Exception ex, string message)
    {
        logger.Info(ex, message);
    }

    /// <summary>
    /// Warn
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Warn(string message, params object[] args)
    {
        logger.Warn(message, args);
    }

    /// <summary>
    /// Warn
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="message"></param>
    public static void Warn(Exception ex, string message)
    {
        logger.Warn(ex, message);
    }

    /// <summary>
    /// Trace
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Trace(string message, params object[] args)
    {
        logger.Trace(message, args);
    }

    /// <summary>
    /// Trace
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="message"></param>
    public static void Trace(Exception ex, string message)
    {
        logger.Trace(ex, message);
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Error(string message, params object[] args)
    {
        logger.Error(message, args);
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="message"></param>
    public static void Error(Exception ex, string message)
    {
        logger.Error(ex, message);
    }

    /// <summary>
    /// Fatal
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Fatal(string message, params object[] args)
    {
        logger.Fatal(message, args);
    }

    /// <summary>
    /// Fatal
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="message"></param>
    public static void Fatal(Exception ex, string message)
    {
        logger.Fatal(ex, message);
    }

    /// <summary>
    /// Flush any pending log messages (in case of asynchronous targets).
    /// </summary>
    /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
    public static void Flush(int? timeoutMilliseconds = null)
    {
        if (timeoutMilliseconds != null)
            LogManager.Flush(timeoutMilliseconds.Value);
        else
            LogManager.Flush();
    }
}

