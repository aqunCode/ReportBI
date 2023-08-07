using Bi.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bi.Core.EasyFS
{
    public interface IEasyFSService
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        ResponseResult<string> UpLoad(AppFileInput input);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        ResponseResult<byte[]> Download(string fileId);
    }
}
