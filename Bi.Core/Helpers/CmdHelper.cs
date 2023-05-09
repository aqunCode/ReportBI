﻿using Bi.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.Helpers;
/// <summary>
/// cmd工具类
/// </summary>
public class CmdHelper
{
    /// <summary>
    /// 执行Linux命令
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static CmdResult Linux(string cmd)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new CmdResult { Error = "The current system does not support it!" };

        var args = cmd.Replace("\"", "\\\"");

        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{args}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            ErrorDialog = false
        };

        return Execute(startInfo);
    }

    /// <summary>
    /// 执行Windows命令
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static CmdResult Windows(params string[] cmd)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new CmdResult { Error = "The current system does not support it!" };

        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            ErrorDialog = false
        };

        if (cmd.IsNotNull())
        {
            var cmds = cmd.ToList();
            cmds.AddIfNotContains("exit");
            cmd = cmds.ToArray();
        }

        return Execute(startInfo, cmd);
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="startInfo"></param>
    /// <param name="cmds"></param>
    /// <returns></returns>
    public static CmdResult Execute(ProcessStartInfo startInfo, string[] cmds = null)
    {
        try
        {
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();

                if (cmds.IsNotNullOrEmpty())
                {
                    foreach (var cmd in cmds)
                    {
                        process.StandardInput.WriteLine(cmd);
                    }
                }

                var result = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                var code = process.ExitCode;

                process.Close();

                return new CmdResult
                {
                    Success = code == 0,
                    Error = error,
                    Output = result
                };
            }
        }
        catch (Exception ex)
        {
            return new CmdResult
            {
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 执行指定exe程序命令
    /// </summary>
    /// <param name="process">进程</param>
    /// <param name="exe">exe可执行文件路径</param>
    /// <param name="arg">参数</param>
    /// <param name="output">委托</param>
    /// <example>
    ///     <code>
    ///         var tool = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\aria2-1.34.0-win-64bit-build1\\aria2c.exe";
    ///         var fi = new FileInfo(strFileName);
    ///         var command = " -c -s 10 -x 10 --file-allocation=none --check-certificate=false -d " + fi.DirectoryName + " -o " + fi.Name + " " + url;
    ///         using (var p = new Process())
    ///         {
    ///             Execute(p, tool, command, (s, e) => ShowInfo(url, e.Data));
    ///         }
    ///     </code>
    /// </example>
    public static void Execute(
        Process process,
        string exe,
        string arg,
        DataReceivedEventHandler output)
    {
        process.StartInfo.FileName = exe;
        process.StartInfo.Arguments = arg;

        //输出信息重定向
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;

        process.OutputDataReceived += output;
        process.ErrorDataReceived += output;

        //启动线程
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        //等待进程结束
        process.WaitForExit();
    }
}

/// <summary>
/// cmd命令返回结果
/// </summary>
public class CmdResult
{
    /// <summary>
    /// 输出内容
    /// </summary>
    public string Output { get; set; }

    /// <summary>
    /// 错误内容
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
}

/// <summary>
/// cmd命令类型
/// </summary>
public enum CmdType
{
    /// <summary>
    /// Windows命令
    /// </summary>
    Windows = 0,

    /// <summary>
    /// Linux命令
    /// </summary>
    Linux = 1
}
